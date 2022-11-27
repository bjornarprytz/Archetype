using System.Linq.Expressions;
using System.Reflection;
using Archetype.Components.Meta;
using Archetype.Core.Effects;
using Archetype.Core.Extensions;

namespace Archetype.Components.Extensions;

internal static class EffectExtensions // TODO: Write tests for these methods
{
    internal static IEffectDescriptor CreateDescriptor<TContext, TResult>(this Expression<Func<TContext, TResult>> exp)
        where TContext : IContext
        where TResult : IResult
    {
        return exp
            .GetMethodCall()
            .ParseMethodCall<TContext>();
    }

    private static MethodCallExpression GetMethodCall<TContext, TResult>(this Expression<Func<TContext, TResult>> exp)
        where TContext : IContext
        where TResult : IResult
    {
        if (exp is not LambdaExpression
            {
                Body: MethodCallExpression mce, Parameters: IReadOnlyCollection<ParameterExpression> parameters
            }
            || !parameters.First().Type.IsAssignableTo(typeof(TContext)))
        {
            throw new ArgumentException(
                "Expression must be a method call, which starts by accessing the resolution context");
        }

        return mce;
    }

    private static IEffectDescriptor ParseMethodCall<TContext>(this MethodCallExpression mce)
        where TContext : IContext
    {
        return 
            mce.Method.Name == nameof(ContextExtensions.TargetEach) 
                ? mce.DescribeMultipleTargets<TContext>() 
                : mce.DescribeSingleTarget<TContext>();
    }

    private static IEffectDescriptor DescribeMultipleTargets<TContext>(this MethodCallExpression mce)
        where TContext : IContext
    {
        var lambda = GetLambda();

        return mce.Arguments.First().TryGetTargetDescriptor(out var targetDescriptor) 
            ? lambda.DescribeLambda<TContext>(targetDescriptor) 
            : lambda.DescribeLambda<TContext>();

        LambdaExpression GetLambda()
        {
            if (mce.Arguments.SecondOrDefault() is not LambdaExpression argLambda)
                throw new ArgumentException($"Cannot parse ForEach on expression {mce}");

            return argLambda;
        }
    }

    private static IEffectDescriptor DescribeSingleTarget<TContext>(this MethodCallExpression mce)
        where TContext : IContext
    {
        if (mce.Object is not { Type: not null } me)
            throw new ArgumentException(
                $"Targeted effect must call a method on an object. Are you using an extension method? {mce}");


        var rulesTemplate = mce.Method.GetRequiredAttribute<KeywordAttribute>().Template;
        
        var operands = mce.Arguments.Select(arg => arg.ParseArgument<TContext>());

        return me.TryGetTargetDescriptor(out var targetDescriptor)
            ? new EffectDescriptor(rulesTemplate, operands, targetDescriptor)
            : new EffectDescriptor(rulesTemplate, operands);
    }

    private static IEffectParameter ParseArgument<TContext>(this Expression exp)
        where TContext : IContext
    {
        return (exp) switch
        {
            ConstantExpression { Value: {} value } => new ImmediateValue(value),
            MethodCallExpression m => m.ParseArgumentMethod<TContext>(),
            MemberExpression a => a.ParseProperty<TContext>(),
            _ => throw new ArgumentException($"Argument of unsupported type {exp}"),
        };
    }

    private static IEffectParameter ParseArgumentMethod<TContext>(this MethodCallExpression mce)
        where TContext : IContext
    {
        if (mce.Arguments.FirstOrDefault() is not { } contextExpression)
            throw new ArgumentException(
                $"Cannot parse first argument of method {mce.Method.Name}, which has {mce.Arguments.Count} arguments");

        var pe = contextExpression.GetRequiredParameterExpressionRootedInContext<TContext>();
        var description = mce.Method.GetRequiredAttribute<DescriptionAttribute>().Description;
        var parameterFunc = Expression.Lambda<Func<TContext, string>>(mce, false, pe).Compile();
        
        return mce.TryGetTargetDescriptor(out var targetDescriptor) 
            ? new ContextParameter<TContext>(parameterFunc, description, targetDescriptor) 
            : new ContextParameter<TContext>(parameterFunc, description);
    }

    private static IEffectParameter ParseProperty<TContext>(this MemberExpression me)
        where TContext : IContext
    {
        var pe = me.GetRequiredParameterExpressionRootedInContext<TContext>();
        var description = me.Member.GetRequiredAttribute<DescriptionAttribute>().Description;
        var parameterFunc = Expression.Lambda<Func<TContext, string>>(me, false, pe).Compile();

        return me.TryGetTargetDescriptor(out var targetDescriptor) 
            ? new ContextParameter<TContext>(parameterFunc, description, targetDescriptor) 
            : new ContextParameter<TContext>(parameterFunc, description);
    }

    private static IEffectDescriptor DescribeLambda<TContext>(this LambdaExpression lambda, ITargetDescriptor? targetDescriptor=null)
        where TContext : IContext
    {
        if (lambda.Body is not MethodCallExpression mce)
            throw new ArgumentException("Lambda body must call a method");

        var rulesTemplate = mce.Method.GetRequiredAttribute<KeywordAttribute>().Template;

        var operands = mce.Arguments.Select(arg => arg.ParseArgument<TContext>());

        return new EffectDescriptor(rulesTemplate, operands, targetDescriptor);
    }

    private static ParameterExpression GetRequiredParameterExpressionRootedInContext<TContext>(this Expression expression)
        where TContext : IContext
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

        if (rootExpression is not ParameterExpression pe || !pe.Type.IsAssignableTo(typeof(TContext)))
        {
            throw new ArgumentException($"Expression {expression} must be rooted in the input context of type {typeof(TContext)}");
        }

        return pe;
    }

    private static bool TryGetTargetDescriptor(this Expression expression, out ITargetDescriptor? targetDescriptor)
    {
        var rootExpression = expression;
        
        while (rootExpression is MemberExpression innerMe)
        {
            rootExpression = innerMe.Expression;
        }

        if (rootExpression is MethodCallExpression { Method.Name: nameof(ContextExtensions.Target) } targetCall)
        {
            if (targetCall.Arguments.SecondOrDefault() is not ConstantExpression { Value: int targetIndex })
                throw new ArgumentException($"Malformed Target expression {expression}");

            targetDescriptor = new TargetProperty(targetCall.Type, targetIndex);
            return true;
        }

        targetDescriptor = null;
        return false;
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

    private record EffectDescriptor(string RulesTemplate, IEnumerable<IEffectParameter> Operands, ITargetDescriptor? MainTarget=null) : IEffectDescriptor;
    private record TargetProperty(Type TargetType, int TargetIndex) : ITargetDescriptor;
    
    private record ContextParameter<TContext> : IEffectParameter
        where TContext : IContext
    {
        private readonly Func<TContext, string> _parameterFunc;
        private readonly List<ITargetDescriptor> _targets = new();
        public ContextParameter(Func<TContext, string> parameterFunc, string description, ITargetDescriptor? targetDescriptor=null)
        {
            _parameterFunc = parameterFunc;
            Description = description;
            
            if (targetDescriptor is not null)
                _targets.Add(targetDescriptor);
        }


        public IEnumerable<ITargetDescriptor> GetTargets() => _targets;

        public string Description { get; }
        public string ComputeValue(TContext context)
        {
            return _parameterFunc(context);
        }
        public string ComputeValue(IContext context)
        {
            if (context is not TContext tContext)
                throw new ArgumentException($"Context {context} is not of type {typeof(TContext)}");
            
            return _parameterFunc(tContext);
        }
    }

    private record ImmediateValue : IEffectParameter
    {
        public ImmediateValue(object value)
        {
            if (value?.ToString() is not { } stringValue)
                throw new ArgumentNullException(nameof(value));
            
            Description = stringValue;
        }

        public IEnumerable<ITargetDescriptor> GetTargets()
        {
            return Enumerable.Empty<ITargetDescriptor>();
        }

        public string Description { get; }

        public string ComputeValue(IContext context)
        {
            return Description;
        }
    }

    
}