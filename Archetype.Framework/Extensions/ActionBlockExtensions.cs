using System.Collections.Frozen;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Core.Structure;
using Archetype.Framework.Design;
using Archetype.Framework.Interface.Actions;
using Archetype.Framework.State;

namespace Archetype.Framework.Extensions;

public static class ActionBlockExtensions
{
    public static void ResolvePaymentsAndQueueEffects(this  IActionBlock actionBlock, IGameRoot gameRoot, IEnumerable<Guid> targetGuids, IEnumerable<PaymentPayload> paymentPayloads)
    {
        var gameState = gameRoot.GameState;
        var metaGameState = gameRoot.MetaGameState;
        var actionQueue = gameRoot.Infrastructure.ActionQueue;
        
        var targets = targetGuids.Select(gameState.GetAtom).ToList();

        var payments = paymentPayloads.ToFrozenDictionary(p => p.Type, p => p.Payment.Select(gameState.GetAtom).ToList() as IReadOnlyList<IAtom>);
        var effects = actionBlock.Effects.Concat(actionBlock.AfterEffects).ToList();
        var costs = actionBlock.Costs;
        
        var resolutionContext = actionBlock.CreateAndValidateResolutionContext(gameState, metaGameState, payments, targets);

        var paymentContext = new PaymentContext(resolutionContext, costs, payments);
        
        if (actionQueue.ResolveCosts(paymentContext) is FailureResult failure)
            throw new InvalidOperationException(failure.Message);
        
        
        actionQueue.Push(new ResolutionFrame(resolutionContext, effects));
    }


    private static IResolutionContext CreateAndValidateResolutionContext(
        this IActionBlock actionBlock, 
        IGameState gameState, 
        IMetaGameState metaGameState, 
        IReadOnlyDictionary<CostType, IReadOnlyList<IAtom>> payments, 
        IReadOnlyList<IAtom> targets
        )
    {
        var rules = metaGameState.Rules;
        
        var resolutionContext = new ResolutionContext(metaGameState, gameState, actionBlock.Source)
        {
            Payments = payments,
            Targets = targets,
            ComputedValues = actionBlock.ComputedValues,
        };
        
        actionBlock.UpdateComputedValues(rules, resolutionContext);
        actionBlock.CheckTargets(resolutionContext);
            
        return resolutionContext;
    }

    private static void CheckTargets(this IActionBlock actionBlock, IResolutionContext context)
    {
        var targetDescriptors = actionBlock?.TargetsDescriptors ?? new List<IKeywordInstance>();
        var targets = context?.Targets ?? new List<IAtom>();

        if (targets.Count != targetDescriptors.Count)
            throw new InvalidOperationException($"Invalid number of targets ({targets.Count} != {targetDescriptors.Count})");

        foreach (var (description, target) in targetDescriptors.Zip(targets))
        {
            if (!FilterAtom(target, description))
                throw new InvalidOperationException($"Target ({target.Id}) does not match the description");
        }

        return;
        
        bool FilterAtom(IAtom atom, IKeywordInstance keywordInstance)
        {
            var rules = context!.MetaGameState.Rules;
            var definition = rules.GetOrThrow<ITargetDefinition>(keywordInstance.Keyword);
            
            
            return definition.GetAllowedTargets(context, keywordInstance).Contains(atom);
        }
    }
    
    
}