using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Aqua.EnumerableExtensions;
using Archetype.Game.Attributes;
using Archetype.Game.Exceptions;
using Archetype.Game.Payloads.PlayContext;

namespace Archetype.Game.Extensions
{
    public static class ExpressionExtensions
    {
	    public static string PrintedRulesText<T>(this Expression<Action<T>> exp)
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
		    if (mce.Method.Name == nameof(EnumerableExtensions.ForEach))
		    {
			    return mce.DescribeForEach(context);
		    }

		    return mce.DescribeAction(context);
	    }

	    
	    // c => c.World.Units.ForEach(a => a.Punch(4))
	    // c => c.World.Units.Where(u => u.Health > 4).ForEach(a => a.Punch(4))
	    private static string DescribeForEach<T>(this MethodCallExpression mce, T context)
		    where  T : IEffectResolutionContext
	    {
		    
		    var (me, lambda) = _getObjectAndLambda(mce);

		    var @object = me.DescribeParameterPath(); // TODO: Do something special here since it's a collection?

		    var verb = lambda.DescribeLambda(context);

		    return $"{verb} {@object}";

		    (MemberExpression, LambdaExpression) _getObjectAndLambda(MethodCallExpression exp)
		    {
			    // {argMe} can be either the lambda
			    // |a => a.Punch(4)|
			    // or a linq method
			    // |c.World.Units.Where(u => u.Health > 4)|
			    // In the latter case, the lambda |a=>a.Punch(4)| will be the second argument

			    if (mce.Object is MemberExpression objMe && mce.Arguments.FirstOrDefault() is LambdaExpression objLambda)
			    {
				    return (objMe, objLambda);
			    }
			    
			    if (mce.Arguments.FirstOrDefault() is MemberExpression argMe && mce.Arguments.Skip(1).FirstOrDefault() is LambdaExpression argLambda)
			    {
				    return (argMe, argLambda);
			    }

			    throw new MalformedEffectException($"Cannot parse ForEach on expression {mce}");
		    }
	    }
	    
	    
	    // c => c.Target.Punch(1)
	    private static string DescribeAction<T>(this MethodCallExpression mce, T context)
		    where  T : IEffectResolutionContext
	    {
		    if (mce.Object is not Expression me || me is not ParameterExpression && me is not MemberExpression)
			    throw new MalformedEffectException($"Targeted effect must call a method on an object. Are you using an extension method? {mce}");

		    var targetAttribute = me.GetRequiredTargetAttribute();

		    var verbAttribute = mce.GetRequiredVerbAttribute();

		    var @object = targetAttribute.Singular;
		    var verb = verbAttribute.Name;

		    return mce.Arguments.Count switch
		    {
			    0 => $"{verb} {@object}",
			    1 => $"{verb} {@object} {verbAttribute.Preposition} {mce.Arguments.Single().DescribeArgument(context)}",
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

	    // e.g. the lambda of a linq method Count(|x => x.Health > 5|)
	    private static string DescribeArgumentMethod<T>(this MethodCallExpression mce, T context)
		    where  T : IEffectResolutionContext
	    {
		    if (mce.Arguments.FirstOrDefault() is not MemberExpression me)
			    throw new MalformedEffectException(
				    $"Cannot describe first argument of method {mce.Method.Name}, which has {mce.Arguments.Count} arguments");

		    var pe = me.GetRequiredRootParameterExpression();

		    if (context is not null && pe.Type.IsAssignableTo(typeof(T)))
		    {
			    var expr = Expression.Lambda<Func<T, int>>(mce, false, pe);
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

		    throw new MalformedEffectException($"Could not parse ArgumentMethod {mce}");
	    }

	    private static string DescribeProperty<T>(this MemberExpression me, T context)
	    {
		    var pe = me.GetRequiredRootParameterExpression();
		    
		    if (context is not null && pe.Type.IsAssignableTo(typeof(T)))
		    {
			    var expr = Expression.Lambda<Func<T, int>>(me, false, pe); // TODO: account for args

			    return expr.Compile().Invoke(context).ToString();
		    }

		    return me.DescribeParameterPath();
	    }

	    private static string DescribeLambda<T>(this LambdaExpression lambda, T context)
		    where  T : IEffectResolutionContext
	    {
		    return lambda.Body switch
		    {
			    BinaryExpression b =>
				    $"{b.Left.DescribeArgument(context)} is {b.NodeType} {b.Right.DescribeArgument(context)}",
			    MethodCallExpression mce => mce.DescribeAction(context)
		    };
	    }
	    
	    // c => c.Player.Hand.Cards would read (each Card in Hand) 
	    private static string DescribeParameterPath(this MemberExpression me)
	    {
		    var sb = new StringBuilder();
		
		    var steps = new List<string> { me.GetRulesDescription() };

		    var innerExpression = me.Expression;

		    var childExpression = me;
		    
		    while (innerExpression is MemberExpression innerMe)
		    {
			    steps.Add($"{innerMe.GetRulesDescription()}.");

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

	    private static ParameterExpression GetRequiredRootParameterExpression(this MemberExpression me)
	    {
		    var innerExpression = me.Expression;
		    while (innerExpression is MemberExpression innerMe)
		    {
			    innerExpression = innerMe.Expression;
		    }

		    if (innerExpression is not ParameterExpression p)
		    {
			    throw new MalformedEffectException($"Member expression {me} must be rooted in a parameter");
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
	    
	    private static TargetAttribute GetRequiredTargetAttribute(this Expression me)
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


	    private static bool MemberIsCollection(this MemberExpression me)
	    {
		    return me.Type.GetInterface(nameof(IEnumerable)) != null;
	    }
    }
}