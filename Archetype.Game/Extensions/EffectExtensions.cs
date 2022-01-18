using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Archetype.Game.Attributes;
using Archetype.Game.Exceptions;
using Archetype.Game.Payloads.Context;
using Archetype.View.Infrastructure;

namespace Archetype.Game.Extensions;

internal static class EffectExtensions
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
        if (mce.Method.Name == nameof(ContextExtensions.TargetEach))
        {
            mce.DescribeMultipleTargets(context);
        }

        return mce.DescribeSingleTarget(context);
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
            if (mce.Arguments.Skip(1).FirstOrDefault() is not LambdaExpression argLambda)
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

        var verbAttribute = mce.Method.GetRequiredAttribute<VerbAttribute>();

        var @object = targetAttribute.Singular;

        var argumentDescriptors = mce.Arguments.Select(arg => arg.DescribeArgument(context));

        return new EffectDescriptor(@object, verbAttribute.Name, argumentDescriptors);
    }

    private static IArgumentDescriptor DescribeArgument<T>(this Expression exp, T context)
        where T : IContext
    {
        return (exp) switch
        {
            ConstantExpression { Value: not null } c => new FullArgumentDescriptor(c.Value.ToString()),
            MethodCallExpression m => m.DescribeArgumentMethod(context),
            MemberExpression a => a.DescribeProperty(context),
            _ => throw new MalformedEffectException($"Argument of unsupported type {exp}"),
        };
    }

    private static IArgumentDescriptor DescribeArgumentMethod<T>(this MethodCallExpression mce, T context)
        where T : IContext
    {
        if (mce.Arguments.FirstOrDefault() is not Expression contextExpression)
            throw new MalformedEffectException(
                $"Cannot describe first argument of method {mce.Method.Name}, which has {mce.Arguments.Count} arguments");

        var pe = contextExpression.GetRequiredRootParameterExpression();

        if (context is not null && pe.Type.IsAssignableTo(typeof(T)))
        {
            var expr = Expression.Lambda<Func<T, int>>(mce, false, pe);
            return new FullArgumentDescriptor(expr.Compile().Invoke(context).ToString());
        }

        return new PartialArgumentDescriptor(mce.Method.GetRequiredAttribute<ContextFactAttribute>().Description);
    }

    private static IArgumentDescriptor DescribeProperty<T>(this MemberExpression me, T context)
    {
        var pe = me.GetRequiredRootParameterExpression();

        if (context is not null && pe.Type.IsAssignableTo(typeof(T)))
        {
            var expr = Expression.Lambda<Func<T, int>>(me, false, pe);

            return new PartialArgumentDescriptor(expr.Compile().Invoke(context).ToString());
        }

        return me.DescribeParameterPath();
    }

    private static IEffectDescriptor DescribeLambda<T>(this LambdaExpression lambda, T context, string groupDescription)
        where T : IContext
    {
        if (lambda.Body is not MethodCallExpression mce)
            throw new MalformedEffectException("Lambda body must call a method");

        var verbAttribute = mce.Method.GetRequiredAttribute<VerbAttribute>();

        var argumentDescriptors = mce.Arguments.Select(arg => arg.DescribeArgument(context));

        return new EffectDescriptor(groupDescription, verbAttribute.Name, argumentDescriptors);
    }

    private static IArgumentDescriptor DescribeParameterPath(this MemberExpression me)
    {
        var propertyName = me.Member.Name;

        var targetDescription = me.Expression switch
        {
            ParameterExpression parameterExpression => me.Member.GetRequiredAttribute<TargetAttribute>().Singular,
            MemberExpression memberExpression =>
                $"target {memberExpression.Type.GetRequiredAttribute<TargetAttribute>().Singular}",
            MethodCallExpression methodCallExpression => methodCallExpression.Method
                .GetRequiredAttribute<ContextPropertyAttribute>().Description,
            _ => throw new ArgumentException($"Indescribable expression {me.Expression}")
        };

        return new PartialArgumentDescriptor($"{propertyName} of {targetDescription}");
    }

    private static ParameterExpression GetRequiredRootParameterExpression(this Expression expression)
    {
        while (expression is MemberExpression innerMe)
        {
            expression = innerMe.Expression;
        }

        if (expression is not ParameterExpression p)
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

    private record EffectDescriptor
        (string Affected, string Keyword, IEnumerable<IArgumentDescriptor> Arguments) : IEffectDescriptor;

    private record FullArgumentDescriptor(string Description) : IArgumentDescriptor
    {
        public bool NeedsMoreContext => false;
    }

    private record PartialArgumentDescriptor(string Description) : IArgumentDescriptor
    {
        public bool NeedsMoreContext => true;
    }
}