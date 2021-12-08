using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Aqua.EnumerableExtensions;
using Archetype.Game.Attributes;
using Archetype.Game.Exceptions;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Context.Effect;

namespace Archetype.Game.Extensions
{
    public static class ExpressionExtensions
    {
	    public static string PrintedRulesText<T, R>(this Expression<Func<T, R>> exp)
			where T : IEffectResolutionContext
			where R : IEffectResult
		{
			return exp
				.GetMethodCall()
				.ParseMethodCall<T>();
		}
	    
	    public static string ContextSensitiveRulesText<T, R>(this Expression<Func<T, R>> exp, T context)
		    where T : IEffectResolutionContext
			where R : IEffectResult
		{
			return exp
				.GetMethodCall()
				.ParseMethodCall(context);
		}

	    private static MethodCallExpression GetMethodCall<T, R>(this Expression<Func<T, R>> exp)
		    where T : IEffectResolutionContext
		    where R : IEffectResult
	    {
		    if (exp is not LambdaExpression { Body: MethodCallExpression mce, Parameters: IReadOnlyCollection<ParameterExpression> parameters } 
		        || !parameters.First().Type.IsAssignableTo(typeof(T)))
		    {
			    throw new MalformedEffectException("Expression must be a method call, which starts by accessing the resolution context");
		    }

		    return mce;
	    }

	    private static string ParseMethodCall<T>(this MethodCallExpression mce, T context=default)
		    where  T : IEffectResolutionContext
	    {
		    if (mce.Method.Name == nameof(ContextExtensions.TargetEach))
		    {
			    return mce.DescribeMultipleTargets(context);
		    }

		    return mce.DescribeSingleTarget(context);
	    }

	    
	    // c => c.World.Units.ForEach(a => a.Punch(4))
	    // c => c.World.Units.Where(u => u.Health > 4).ForEach(a => a.Punch(4))
	    private static string DescribeMultipleTargets<T>(this MethodCallExpression mce, T context)
		    where  T : IEffectResolutionContext
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
	    
	    
	    // c => c.Target.Punch(1)
	    private static string DescribeSingleTarget<T>(this MethodCallExpression mce, T context)
		    where  T : IEffectResolutionContext
	    {
		    if (mce.Object is not Expression me || me is not ParameterExpression && me is not MemberExpression)
			    throw new MalformedEffectException($"Targeted effect must call a method on an object. Are you using an extension method? {mce}");

		    var targetAttribute = me.Type.GetRequiredAttribute<TargetAttribute>();

		    var templateAttr = mce.Method.GetRequiredAttribute<TemplateAttribute>();

		    var @object = targetAttribute.Singular;
		    var template = templateAttr.Template;

		    return mce.Arguments.Count switch
		    {
			    0 => string.Format(template, @object),
			    1 => string.Format(template, @object, mce.Arguments.Single().DescribeArgument(context)),
			    _ => throw new MalformedEffectException($"Only methods with 1 or 0 arguments are supported. {mce.Method} has {mce.Arguments.Count}")
		    };
	    }

	    private static string DescribeArgument<T>(this Expression exp, T context)
		    where  T : IEffectResolutionContext
	    {
		    return (exp) switch
		    {
			    ConstantExpression { Value: not null } c => c.Value.ToString(),
			    MethodCallExpression m => m.DescribeArgumentMethod(context),
			    MemberExpression a => a.DescribeProperty(context),
			    _ => throw new MalformedEffectException($"Argument of unsupported type {exp}"),
		    };
	    }

	    private static string DescribeArgumentMethod<T>(this MethodCallExpression mce, T context)
		    where  T : IEffectResolutionContext
	    {
		    if (mce.Arguments.FirstOrDefault() is not Expression contextExpression)
			    throw new MalformedEffectException(
				    $"Cannot describe first argument of method {mce.Method.Name}, which has {mce.Arguments.Count} arguments");

		    var pe = contextExpression.GetRequiredRootParameterExpression();

		    if (context is not null && pe.Type.IsAssignableTo(typeof(T)))
		    {
			    var expr = Expression.Lambda<Func<T, int>>(mce, false, pe);
			    return expr.Compile().Invoke(context).ToString();			    
		    }

		    return mce.Method.GetRequiredAttribute<ContextFactAttribute>().Description;
	    }

	    private static string DescribeProperty<T>(this MemberExpression me, T context)
	    {
		    var pe = me.GetRequiredRootParameterExpression();
		    
		    if (context is not null && pe.Type.IsAssignableTo(typeof(T)))
		    {
			    var expr = Expression.Lambda<Func<T, int>>(me, false, pe);

			    return expr.Compile().Invoke(context).ToString();
		    }

		    return me.DescribeParameterPath(context);
	    }

	    private static string DescribeLambda<T>(this LambdaExpression lambda, T context, string groupDescription)
		    where  T : IEffectResolutionContext
	    {
		    if (lambda.Body is not MethodCallExpression mce)
			    throw new MalformedEffectException("Lambda body must call a method");
		    
		    var templateAttr = mce.Method.GetRequiredAttribute<TemplateAttribute>();

		    var template = templateAttr.Template;

		    return mce.Arguments.Count switch
		    {
			    0 => string.Format(template, groupDescription),
			    1 => string.Format(template, groupDescription, mce.Arguments.Single().DescribeArgument(context)),
			    _ => throw new MalformedEffectException($"Only methods with 1 or 0 arguments are supported. {mce.Method} has {mce.Arguments.Count}")
		    };
	    }
	    
	    private static string DescribeParameterPath<T>(this MemberExpression me, T context)
	    {
		    var sb = new StringBuilder();
		
		    var steps = new List<string> { me.Member.Name };

		    var innerExpression = me.Expression;

		    var childExpression = me;
		    
		    while (innerExpression is MemberExpression innerMe)
		    {
			    steps.Add($"{innerMe.Member.Name}.");

			    innerExpression = innerMe.Expression;	// Hand
			    childExpression = innerMe;				// Cards
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
			where  T : Attribute
	    {
		    var targetAttr = memberInfo.GetCustomAttribute<T>();

		    if (targetAttr is null)
			    throw new MalformedEffectException(
				    $"Member {memberInfo} should be decorated with {typeof(T)}");

		    return targetAttr;
	    }
    }
}