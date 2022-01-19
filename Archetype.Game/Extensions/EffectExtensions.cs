using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Aqua.EnumerableExtensions;
using Archetype.Game.Attributes;
using Archetype.Game.Exceptions;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Context.Card;
using Archetype.View.Infrastructure;
using OneOf;

namespace Archetype.Game.Extensions;

public static class EffectExtensions
{
    public static IEffectDescriptor CreateDescriptor<T, R>(this Expression<Func<T, R>> exp)
        where T : IContext
        where R : IResult
    {
        return exp
            .GetMethodCall()
            .ParseMethodCall<T>();
    }

    public static IEffectDescriptor CreateDescriptor<T, R>(this Expression<Func<T, R>> exp, T context)
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
            throw new MalformedEffectException(
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
        var group = GetGroup();

        return lambda.DescribeLambda(context, group);

        string GetGroup()
        {
            if (mce.Arguments.FirstOrDefault() is not MethodCallExpression argMce)
                throw new MalformedEffectException("Group accessor must be a method call");

            return argMce.Method.GetRequiredAttribute<GroupAttribute>().Description;
        }

        LambdaExpression GetLambda()
        {
            if (mce.Arguments.SecondOrDefault() is not LambdaExpression argLambda)
                throw new MalformedEffectException($"Cannot parse ForEach on expression {mce}");

            return argLambda;
        }
    }

    private static IEffectDescriptor DescribeSingleTarget<T>(this MethodCallExpression mce, T context)
        where T : IContext
    {
        if (mce.Object is not { Type: not null } me)
            throw new MalformedEffectException(
                $"Targeted effect must call a method on an object. Are you using an extension method? {mce}");

        var targetAttribute = me.Type.GetRequiredAttribute<TargetAttribute>();

        var verbAttribute = mce.Method.GetRequiredAttribute<KeywordAttribute>();

        var @object = targetAttribute.Singular;

        var operands = mce.Arguments.Select(arg => arg.ParseArgument(context));

        return new EffectDescriptor(@object, verbAttribute.Name, operands);
    }

    private static IOperand ParseArgument<T>(this Expression exp, T context)
        where T : IContext
    {
        return (exp) switch
        {
            ConstantExpression { Value: not null } c => new Operand(new ImmediateValue(c.Value.ToString())),
            MethodCallExpression m => m.ParseArgumentMethod(context),
            MemberExpression a => a.ParseProperty(context),
            _ => throw new MalformedEffectException($"Argument of unsupported type {exp}"),
        };
    }

    private static IOperand ParseArgumentMethod<T>(this MethodCallExpression mce, T context)
        where T : IContext
    {
        if (mce.Arguments.FirstOrDefault() is not Expression contextExpression)
            throw new MalformedEffectException(
                $"Cannot describe first argument of method {mce.Method.Name}, which has {mce.Arguments.Count} arguments");

        var pe = contextExpression.GetRequiredRootParameterExpression();

        if (context is not null && pe.Type.IsAssignableTo(typeof(T)))
        {
            var expr = Expression.Lambda<Func<T, int>>(mce, false, pe);
            return new Operand(new ImmediateValue(expr.Compile().Invoke(context).ToString()));
        }

        return mce.DescribeParameterPath();
    }

    private static IOperand ParseProperty<T>(this MemberExpression me, T context)
    {
        var pe = me.GetRequiredRootParameterExpression();

        if (context is not null && pe.Type.IsAssignableTo(typeof(T)))
        {
            var expr = Expression.Lambda<Func<T, int>>(me, false, pe);

            return new Operand(new ImmediateValue(expr.Compile().Invoke(context).ToString()));
        }

        return me.DescribeParameterPath();
    }

    private static IEffectDescriptor DescribeLambda<T>(this LambdaExpression lambda, T context, string groupDescription)
        where T : IContext
    {
        if (lambda.Body is not MethodCallExpression mce)
            throw new MalformedEffectException("Lambda body must call a method");

        var verbAttribute = mce.Method.GetRequiredAttribute<KeywordAttribute>();

        var operands = mce.Arguments.Select(arg => arg.ParseArgument(context));

        return new EffectDescriptor(groupDescription, verbAttribute.Name, operands);
    }

    private static IOperand DescribeParameterPath(this Expression expression)
    {
        var (rootExpression, propertyPath) = RootExpressionAndPropertyPath();

        switch (rootExpression)
        {
            case ParameterExpression:
                return new Operand(new ContextProperty(propertyPath));
            
            case MethodCallExpression { Method.Name: nameof(ContextExtensions.Target) } targetCall:
            {
                var targetIndex = targetCall.Arguments.SecondOrDefault() is ConstantExpression ce ? (int)ce.Value! : 0;

                return new Operand(new TargetProperty(targetCall.Type, targetIndex, propertyPath));
            }
            case MethodCallExpression mce:
                return new Operand(
                    new AggregateProperty(mce.Method.GetRequiredAttribute<ContextFactAttribute>().Description, propertyPath));
            default:
                throw new ArgumentException($"Indescribable expression {expression}");
        }

        (Expression, string) RootExpressionAndPropertyPath()
        {
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
            throw new MalformedEffectException($"Expression {expression} must be rooted in a parameter");
        }

        return p;
    }

    private static T GetRequiredAttribute<T>(this MemberInfo memberInfo)
        where T : Attribute
    {
        var requiredAttribute = memberInfo.GetCustomAttribute<T>();

        if (requiredAttribute is null)
            throw new MalformedEffectException(
                $"Member {memberInfo} should be decorated with {typeof(T)}");

        return requiredAttribute;
    }

    private record EffectDescriptor(string Affected, string Keyword, IEnumerable<IOperand> Arguments) : IEffectDescriptor;

    private record Operand : IOperand
    {
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