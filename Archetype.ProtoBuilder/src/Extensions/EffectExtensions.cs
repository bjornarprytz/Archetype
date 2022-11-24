using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Archetype.Components.Meta;
using Archetype.Core.Effects;
using Archetype.Core.Extensions;
using OneOf;

namespace Archetype.Components.Extensions;

public static class EffectExtensions
{
    internal static IEffectDescriptor CreateDescriptor<T, R>(this Expression<Func<T, R>> exp)
        where T : IContext
        where R : IResult
    {
        return exp
            .GetMethodCall()
            .ParseMethodCall<T>();
    }

    internal static IEffectDescriptor CreateDescriptor<T, R>(this Expression<Func<T, R>> exp, T context)
        where T : IContext
        where R : IResult
    {
        return exp
            .GetMethodCall()
            .ParseMethodCall(context);
    }

    private static MethodCallExpression GetMethodCall<T, R>(this Expression<Func<T, R>> exp)
        where T : IContext
        where R : IResult
    {
        if (exp is not LambdaExpression
            {
                Body: MethodCallExpression mce, Parameters: IReadOnlyCollection<ParameterExpression> parameters
            }
            || !parameters.First().Type.IsAssignableTo(typeof(T)))
        {
            throw new ArgumentException(
                "Expression must be a method call, which starts by accessing the resolution context");
        }

        return mce;
    }

    private static IEffectDescriptor ParseMethodCall<T>(this MethodCallExpression mce, T context = default)
        where T : IContext
    {
        return 
            mce.Method.Name == nameof(ContextExtensions.TargetEach) 
                ? mce.DescribeMultipleTargets(context) 
                : mce.DescribeSingleTarget(context);
    }


    // c => c.World.Units.ForEach(a => a.Punch(4))
    // c => c.World.Units.Where(u => u.Health > 4).ForEach(a => a.Punch(4))
    private static IEffectDescriptor DescribeMultipleTargets<T>(this MethodCallExpression mce, T context)
        where T : IContext
    {
        var lambda = GetLambda();
        var affected = GetAffected();

        return lambda.DescribeLambda(context, affected);

        IAffected GetAffected()
        {
            return mce.Arguments.First().ParseAffected();
        }
        
        LambdaExpression GetLambda()
        {
            if (mce.Arguments.SecondOrDefault() is not LambdaExpression argLambda)
                throw new ArgumentException($"Cannot parse ForEach on expression {mce}");

            return argLambda;
        }
    }

    private static IEffectDescriptor DescribeSingleTarget<T>(this MethodCallExpression mce, T context)
        where T : IContext
    {
        if (mce.Object is not { Type: not null } me)
            throw new ArgumentException(
                $"Targeted effect must call a method on an object. Are you using an extension method? {mce}");

        var affected = me.ParseAffected();

        var verb = mce.Method.Name;
        
        var operands = mce.Arguments.Select(arg => arg.ParseArgument(context));

        return new EffectDescriptor(affected, verb, operands);
    }

    private static IEffectParameter ParseArgument<T>(this Expression exp, T context)
        where T : IContext
    {
        return (exp) switch
        {
            ConstantExpression { Value: not null } c => new Operand(new ImmediateValue(c.Value.ToString())),
            MethodCallExpression m => m.ParseArgumentMethod(context),
            MemberExpression a => a.ParseProperty(context),
            _ => throw new ArgumentException($"Argument of unsupported type {exp}"),
        };
    }

    private static IEffectParameter ParseArgumentMethod<T>(this MethodCallExpression mce, T context)
        where T : IContext
    {
        if (mce.Arguments.FirstOrDefault() is not Expression contextExpression)
            throw new ArgumentException(
                $"Cannot describe first argument of method {mce.Method.Name}, which has {mce.Arguments.Count} arguments");

        var pe = contextExpression.GetRequiredRootParameterExpression();

        if (context is not null && pe.Type.IsAssignableTo(typeof(T)))
        {
            var expr = Expression.Lambda<Func<T, int>>(mce, false, pe);
            return new Operand(new ImmediateValue(expr.Compile().Invoke(context).ToString()));
        }

        return mce.ParseOperand();
    }

    private static IEffectParameter ParseProperty<T>(this MemberExpression me, T context)
    {
        var pe = me.GetRequiredRootParameterExpression();

        if (context is not null && pe.Type.IsAssignableTo(typeof(T)))
        {
            var expr = Expression.Lambda<Func<T, int>>(me, false, pe);

            return new Operand(new ImmediateValue(expr.Compile().Invoke(context).ToString()));
        }

        return me.ParseOperand();
    }

    private static IEffectDescriptor DescribeLambda<T>(this LambdaExpression lambda, T context, IAffected affected)
        where T : IContext
    {
        if (lambda.Body is not MethodCallExpression mce)
            throw new ArgumentException("Lambda body must call a method");

        var verb = mce.Method.Name;

        var operands = mce.Arguments.Select(arg => arg.ParseArgument(context));

        return new EffectDescriptor(affected, verb, operands);
    }

    private static IAffected ParseAffected(this Expression expression)
    {
        return new Affected(expression.ParseParameterPath());
    }
    
    private static IEffectParameter ParseOperand(this Expression expression)
    {
        return new Operand(expression.ParseParameterPath());
    }

    private static OneOf<IContextProperty, ITargetProperty, IAggregateProperty> ParseParameterPath(this Expression expression)
    {
        var (rootExpression, propertyPath) = RootExpressionAndPropertyPath();

        switch (rootExpression)
        {
            case ParameterExpression:
                return new ContextProperty(propertyPath);
            
            case MethodCallExpression { Method.Name: nameof(ContextExtensions.Target) } targetCall:
            {
                var targetIndex = targetCall.Arguments.SecondOrDefault() is ConstantExpression ce ? (int)ce.Value! : 0;

                return new TargetProperty(targetCall.Type, targetIndex, propertyPath);
            }
            case MethodCallExpression mce:
                return new AggregateProperty(mce.Method.GetRequiredAttribute<ContextFactAttribute>().Description, propertyPath);
            default:
                throw new ArgumentException($"Indescribable expression {expression}");
        }

        (Expression, string) RootExpressionAndPropertyPath()
        {
            if (expression is MethodCallExpression { Method: { } methodInfo } )
            {
                if (methodInfo.GetCustomAttribute<PropertyShortHandAttribute>() is { Path: {} path } )
                {
                    return (expression, path); 
                }
            }
            
            var stack = new Stack<string>();

            var parentExpression = expression;
        
            while (parentExpression is MemberExpression innerMe)
            {
                stack.Push(innerMe.Member.Name);

                parentExpression = innerMe.Expression;
            }
        
            var propertyPathSb = new StringBuilder();

            while (stack.Any())
            {
                propertyPathSb.Append('.');
                propertyPathSb.Append(stack.Pop());
            }
            
            return (parentExpression, propertyPathSb.ToString());
        }
    }

    private static ParameterExpression GetRequiredRootParameterExpression(this Expression expression)
    {
        var rootExpression = expression;
        
        while (rootExpression is MemberExpression innerMe)
        {
            rootExpression = innerMe.Expression;
        }

        if (rootExpression is MethodCallExpression mce )
        {
            rootExpression = mce.Object                 // MemberMethod call
                             ?? mce.Arguments.First();  // Extension method
        }

        if (rootExpression is not ParameterExpression p)
        {
            throw new ArgumentException($"Expression {expression} must be rooted in a parameter");
        }

        return p;
    }

    private static T GetRequiredAttribute<T>(this MemberInfo memberInfo)
        where T : Attribute
    {
        var requiredAttribute = memberInfo.GetCustomAttribute<T>();

        if (requiredAttribute is null)
            throw new ArgumentException(
                $"Member {memberInfo} should be decorated with {typeof(T)}");

        return requiredAttribute;
    }

    private record EffectDescriptor(IAffected Affected, string Keyword, IEnumerable<IEffectParameter> Operands) : IEffectDescriptor;


    private record Affected : IAffected 
    {
        public Affected(IOneOf oneOf)
        {
            Description = oneOf.Value switch
            {
                ITargetProperty tp => OneOf<ITargetProperty, IContextProperty>
                    .FromT0(tp),
                IContextProperty cp => OneOf<ITargetProperty, IContextProperty>
                    .FromT1(cp),
                _ => throw new ArgumentException($"Cannot parse affected of value type {oneOf.Value} "),
            };
        }
    
        public Affected(ITargetProperty description)
        {
            Description = OneOf<ITargetProperty, IContextProperty>.FromT0(description);
        }
    
        public Affected(IContextProperty description)
        {
            Description = OneOf<ITargetProperty, IContextProperty>.FromT1(description);
        }
    
        public OneOf<ITargetProperty, IContextProperty> Description { get; }
    }
    
    private record Operand : IEffectParameter
    {
        public Operand(IOneOf oneOf)
        {
            Value = oneOf.Value switch
            {
                IImmediateValue iv => OneOf<IImmediateValue, ITargetProperty, IContextProperty, IAggregateProperty>
                    .FromT0(iv),
                ITargetProperty tp => OneOf<IImmediateValue, ITargetProperty, IContextProperty, IAggregateProperty>
                    .FromT1(tp),
                IContextProperty cp => OneOf<IImmediateValue, ITargetProperty, IContextProperty, IAggregateProperty>
                    .FromT2(cp),
                IAggregateProperty ap => OneOf<IImmediateValue, ITargetProperty, IContextProperty, IAggregateProperty>
                    .FromT3(ap),
                _ => throw new ArgumentException($"Cannot parse operand of value type {oneOf.Value} "),
            };
        }
    
        public Operand(IImmediateValue value)
        {
            Value = OneOf<IImmediateValue, ITargetProperty, IContextProperty, IAggregateProperty>.FromT0(value);
        }
    
        public Operand(ITargetProperty value)
        {
            Value = OneOf<IImmediateValue, ITargetProperty, IContextProperty, IAggregateProperty>.FromT1(value);
        }

        public Operand(IContextProperty value)
        {
            Value = OneOf<IImmediateValue, ITargetProperty, IContextProperty, IAggregateProperty>.FromT2(value);
        }

        public Operand(IAggregateProperty value)
        {
            Value = OneOf<IImmediateValue, ITargetProperty, IContextProperty, IAggregateProperty>.FromT3(value);
        }
    
        public OneOf<IImmediateValue, ITargetProperty, IContextProperty, IAggregateProperty> Value { get; }
    }

    private record ImmediateValue(string Value) : IImmediateValue;

    private record TargetProperty(Type TargetType, int TargetIndex, string PropertyPath) : ITargetProperty;
    private record ContextProperty(string PropertyPath) : IContextProperty;

    private record AggregateProperty(string Description, string PropertyPath) : IAggregateProperty;
}