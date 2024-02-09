using Archetype.Framework.Core.Primitives;

namespace Archetype.Framework.Extensions;

public static class ContextExtensions
{
    public static IEffectResult MakeDryRuns(this IPaymentContext context)
    {
        var queue = new Queue<IKeywordInstance>(context.Costs);

        while (queue.TryDequeue(out var nextKeyword))
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
        
        var queue = new Queue<IKeywordInstance>(context.Costs);

        var eventStack = new Stack<EffectEvent>();
        
        while (queue.TryDequeue(out var nextKeyword))
        {
            var costDefinition = context.ResolutionContext.MetaGameState.Rules.GetOrThrow<ICostDefinition>(nextKeyword.Keyword);
            var payment = context.Payments[costDefinition.CostType];

            var result = costDefinition.Pay(context.ResolutionContext, nextKeyword, payment); 
            
            if (result is FailureResult failure)
            {
                throw new InvalidOperationException($"Payment failed after dry run succeeded. (reason: {failure.Message})");
            }
            
            if (result is IKeywordFrame { Effects: { } nestedKeywords })
            {
                foreach (var keywordInstance in nestedKeywords)
                {
                    queue.Enqueue(keywordInstance);
                }
                
                eventStack.Push(new EffectEvent(source, nextKeyword, result));
                
                continue;
            }
            
            var nextEvent = new EffectEvent(source, nextKeyword, result);
            
            while (eventStack.TryPop(out var nestedEvent) && nestedEvent.Result is IKeywordFrame keywordFrame)
            {
                if (keywordFrame.Effects.Any(e => e.Id == nextKeyword.Id))
                {
                    nestedEvent.Children.Add(nextEvent);
                    nextEvent.Parent = nestedEvent;
                    
                    eventStack.Push(nestedEvent);
                    break;
                }

                yield return nestedEvent;
            }
            
            if (eventStack.Count == 0)
            {
                yield return nextEvent;
            }
        }
    }
}