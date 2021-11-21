using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Aqua.EnumerableExtensions;
using Aqua.TypeExtensions;
using Archetype.Game.Attributes;
using Archetype.Game.Exceptions;
using Archetype.Game.Payloads.PlayContext;

namespace Archetype.Game.Extensions
{
    public static class ExpressionExtensions
    {
	    public static string PrintedRulesText<T>(this Expression<Action<IEffectResolutionContext>> exp)
			where T : IEffectResolutionContext
		{
			return exp
				.GetMethodCall()
				.ParseMethodCall<T>();
		}
	    
	    public static string ContextSensitiveRulesText<T>(this Expression<Action<T>> exp, T context)
			where T : IEffectResolutionContext
		{
			return exp
				.GetMethodCall()
				.ParseMethodCall(context);
		}

	    private static MethodCallExpression GetMethodCall<T>(this Expression<Action<T>> exp)
			where  T : IEffectResolutionContext
	    {
		    // Is -I-ReadonlyCollection fine to check here?
		    if (exp is not LambdaExpression { Body: MethodCallExpression mce, Parameters: IReadOnlyCollection<ParameterExpression> parameters } 
		        || !parameters.Single().Type.IsAssignableTo(typeof(T)))
		    {
			    throw new MalformedEffectException("Expression must be a method call, which starts by accessing the resolution context");
		    }

		    return mce;
	    }

	    private static string ParseMethodCall<T>(this MethodCallExpression mce, T context=default)
		    where  T : IEffectResolutionContext
	    {
		    if (mce.Method.Name == nameof(EnumerableExtensions.ForEach))
		    {
			    return mce.DescribeForEach(context);
		    }

		    return mce.DescribeTarget(context);
	    }

	    private static string DescribeForEach<T>(this MethodCallExpression mce, T context)
		    where  T : IEffectResolutionContext
	    {
		    if (mce.Arguments.FirstOrDefault() is not MemberExpression me)
			    throw new MalformedEffectException(
				    $"Cannot describe first argument of method {mce.Method.Name}, which has {mce.Arguments.Count} arguments");

		    var pe = me.GetRequiredRootParameterExpression();
		    
		    var group = pe.Des
	    }
	    
	    private static string DescribeTarget<T>(this MethodCallExpression mce, T context)
		    where  T : IEffectResolutionContext
	    {
		    if (mce.Object is not MemberExpression me)
			    throw new MalformedEffectException("Targeted effect must call a method on an object");

		    var @object = me.DescribeMember(context);

		    var verbAttribute = mce.GetRequiredVerbAttribute();

		    var verb = verbAttribute.Name;

		    return mce.Arguments.Count switch
		    {
			    0 => $"{verb} {@object}",
			    1 => $"{verb} {@object} {verbAttribute.Preposition} {mce.Arguments.Single().DescribeArgument(context)}",
			    _ => throw new MalformedEffectException($"Only methods with 1 or 0 arguments are supported. {mce.Method} has {mce.Arguments.Count}")
		    };
	    }

	    private static string DescribeArgument<T>(this Expression exp, T context)
	    {
		    return (exp) switch
		    {
			    ConstantExpression { Value: not null } c => c.Value.ToString(),
			    MethodCallExpression m => m.DescribeArgumentMethod(context),
			    MemberExpression a => a.DescribeProperty(context),
			    _ => throw new MalformedEffectException($"Argument of unsupported type {exp}"),
		    };
	    }

	    private static string DescribeMember<T>(this MemberExpression me, T gameState)
		    where  T : IEffectResolutionContext
	    {
		    var targetAttr = me.GetRequiredTargetAttribute();

		    // TODO: Describe path to the target (from the Context)
		    
			return targetAttr.Singular;
	    }

	    private static string DescribeArgumentMethod<T>(this MethodCallExpression mce, T context)
	    {
		    if (mce.Arguments.FirstOrDefault() is not MemberExpression me)
			    throw new MalformedEffectException(
				    $"Cannot describe first argument of method {mce.Method.Name}, which has {mce.Arguments.Count} arguments");

		    var pe = me.GetRequiredRootParameterExpression();

		    if (context is not null)
		    {
			    var expr = Expression.Lambda<Func<T, int>>(mce, false, pe); // TODO: account for args
			    return expr.Compile().Invoke(context).ToString();			    
		    }

		    var verbAttr = mce.CheckVerbAttribute();

		    if (verbAttr is not null)
		    {
				return $"{verbAttr.Name} {me.DescribeParameterPath()}";
		    }

		    if (mce.Method.ReturnType == typeof(int) && mce.Arguments.Skip(1).FirstOrDefault() is LambdaExpression lambda)
		    {
			    var qualifier = lambda.DescribeLambda(context);
			    
			    return $"{mce.Method.Name} where {qualifier}"; // Count where target.health is less than 2
		    }
		    
		    // TODO: remember to describe linq args (binary expressions

		    return "Under ocnstruntction";
	    }

	    private static string DescribeProperty<T>(this MemberExpression me, T context)
	    {
		    var pe = me.GetRequiredRootParameterExpression();
		    
		    if (context is not null)
		    {
			    var expr = Expression.Lambda<Func<T, int>>(me, false, pe); // TODO: account for args

			    return expr.Compile().Invoke(context).ToString();
		    }

		    return me.DescribeParameterPath();
	    }

	    private static string DescribeLambda<T>(this LambdaExpression lambda, T context)
	    {
		    if (lambda.Body is BinaryExpression b)
		    {
			    return $"{b.Left.DescribeArgument(context)} is {b.NodeType} {b.Right.DescribeArgument(context)}";
		    }
		    else
		    {
			    return "[Indescribable lambda]";
		    }
	    }
	    
	    private static string DescribeParameterPath(this MemberExpression me)
	    {
		    var sb = new StringBuilder();
		
		    var steps = new List<string>();

		    steps.Add(me.GetRulesDescription());

		    var innerExpression = me.Expression;

		    while (innerExpression is MemberExpression innerMe)
		    {
			    steps.Add($"{innerMe.GetRulesDescription()}.");

			    innerExpression = innerMe.Expression;
		    }

		    if (innerExpression is ParameterExpression p)
		    {
			    steps.Add($"{p}."); // TODO: Check that this looks good
		    }

		    steps.Reverse();

		    foreach (var step in steps)
		    {
			    sb.Append(step);
		    }

		    return sb.ToString();
	    }

	    private static ParameterExpression GetRequiredRootParameterExpression(this MemberExpression me)
	    {
		    var innerExpression = me.Expression;
		    while (innerExpression is MemberExpression innerMe)
		    {
			    innerExpression = innerMe.Expression;
		    }

		    if (innerExpression is not ParameterExpression p)
		    {
			    throw new MalformedEffectException($"Member expression must be rooted in a parameter");
		    }

		    return p;
	    }

	    private static VerbAttribute GetRequiredVerbAttribute(this MethodCallExpression mce)
	    {
		    var methodAttr = mce.CheckVerbAttribute();

		    if (methodAttr is null)
		    {
			    throw new MalformedEffectException(
				    $"Method type {mce.Method} on type {mce.Object?.Type} should be decorated with {typeof(VerbAttribute)}");
		    }

		    return methodAttr;
	    }
	    
	    private static VerbAttribute CheckVerbAttribute(this MethodCallExpression mce)
	    {
		    return mce.Method.GetCustomAttribute<VerbAttribute>();
	    }
	    
	    private static TargetAttribute GetRequiredTargetAttribute(this MemberExpression me)
	    {
		    var targetAttr = me.Type.GetCustomAttribute<TargetAttribute>();

		    if (targetAttr is null)
			    throw new MalformedEffectException(
				    $"Target type {me.Type} should be decorated with {typeof(TargetAttribute)}");

		    return targetAttr;
	    }

	    private static string GetRulesDescription(this MemberExpression me)
	    {
		    var rulesDescriptionAttr = me.Type.GetCustomAttribute<RulesDescriptionAttribute>();

		    return rulesDescriptionAttr?.Word ?? me.Member.Name; // [me.Type.Name] could also make sense here (TODO: Iron this out)
	    }
    }
}