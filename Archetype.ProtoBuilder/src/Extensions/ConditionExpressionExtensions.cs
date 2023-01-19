using System.Linq.Expressions;
using Archetype.Components.Meta;
using Archetype.Core.Effects;
using Archetype.Core.Extensions;
using Archetype.Core.Meta;

namespace Archetype.Components.Extensions;

internal static class ConditionExpressionExtensions
{
    internal static IConditionDescriptor CreateDescriptor<TContext>(this Expression<Func<TContext, IEffectResult, bool>> exp)
        where TContext : IContext
    {
        return exp
            .GetMethodCall()
            .ParseMethodCall<TContext>();
        
        // TODO: Parse the method. The method can be constrained to have an attribute (e.g. [Condition])
        // TODO: The keyword that triggers the condition should be in the IEffectResult
        // If there is a predicate, it could could depend on the context.
        
        
        // TODO: Maybe the condition should have to specify where the card needs to be in order to be able to trigger.
    }

}