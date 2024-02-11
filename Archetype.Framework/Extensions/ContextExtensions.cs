using Archetype.Framework.Core.Primitives;

namespace Archetype.Framework.Extensions;

public static class ContextExtensions
{
    
    public static IEffectResult MakeDryRuns(this IPaymentContext context)
    {
        foreach (var nextKeyword in context.Costs)
        {
            var costDefinition = context.ResolutionContext.MetaGameState.Rules.GetOrThrow<ICostDefinition>(nextKeyword.Keyword);
            var payment = context.Payments[costDefinition.CostType];
            
            if (costDefinition.DryRun(context.ResolutionContext, nextKeyword, payment) is FailureResult failure)
            {
                return failure;
            }
        }
        
        return EffectResult.Resolved;
    }
    
    public static IEnumerable<IEvent> ResolvePayments(this IPaymentContext context)
    {
        var source = context.ResolutionContext.Source;
        
        var keywordStack = new Stack<IKeywordInstance>(context.Costs);

        var eventStack = new Stack<EffectEvent>();
        
        while (keywordStack.TryPop(out var nextKeyword) && context.ResolutionContext.MetaGameState.Rules.GetDefinition(nextKeyword.Keyword) is { } definition)
        {
            var result = definition switch
            {
                ICostDefinition costDefinition => costDefinition.Pay(context.ResolutionContext, nextKeyword, context.Payments[costDefinition.CostType]),
                IEffectDefinition effectDefinition => effectDefinition.Resolve(context.ResolutionContext, nextKeyword),
                _ => throw new InvalidOperationException($"Keyword ({nextKeyword.Keyword}) is not a valid definition")
            };
            
            if (result is FailureResult failure)
            {
                throw new InvalidOperationException($"Payment failed after dry run succeeded. (reason: {failure.Message})");
            }
            
            if (result is IKeywordFrame { Effects: { } nestedKeywords })
            {
                foreach (var keywordInstance in nestedKeywords.Reverse())
                {
                    keywordStack.Push(keywordInstance);
                }
                
                eventStack.Push(new EffectEvent(source, nextKeyword, result));
                
                continue;
            }
            
            var nextEvent = new EffectEvent(source, nextKeyword, result);
            
            if (eventStack.Count == 0)
            {
                yield return nextEvent;
            }
            
            while (eventStack.TryPop(out var nestedEvent) && nestedEvent.Result is IKeywordFrame keywordFrame)
            {
                if (keywordFrame.Effects.All(e => e.Id != nextKeyword.Id)) 
                    throw new InvalidOperationException("Nested keyword not found in keyword frame");
                
                nestedEvent.Children.Add(nextEvent);
                nextEvent.Parent = nestedEvent;
                    
                if (keywordFrame.Effects.Count > nestedEvent.Children.Count)
                {
                    eventStack.Push(nestedEvent);
                    break;
                }
                    
                yield return nestedEvent;
            }
            
        }
    }
}