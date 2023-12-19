using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Core.Structure;
using Archetype.Framework.Design;
using Archetype.Framework.Interface.Actions;
using Archetype.Framework.State;

namespace Archetype.Framework.Extensions;

public static class RuntimeExtensions
{
    
    public static IResolutionContext CreateAndValidateResolutionContext(
        this IActionBlock actionBlock, 
        IGameState gameState, 
        IMetaGameState metaGameState, 
        IReadOnlyList<PaymentPayload> payments, 
        IReadOnlyList<IAtom> targets
        )
    {
        var rules = metaGameState.Rules;
        var conditions = actionBlock.Conditions;
        var costs = actionBlock.Costs;
        
        var resolutionContext = new ResolutionContext
        {
            MetaGameState = metaGameState,
            GameState = gameState,
            Payments = payments.ToDictionary(k => k.Type, v => v),
            Source = actionBlock.Source,
            Targets = targets,
            ComputedValues = actionBlock.ComputedValues,
        };
        
        actionBlock.UpdateComputedValues(rules, resolutionContext);
        
        if (!rules.CheckPayments(resolutionContext, costs, payments))
            throw new InvalidOperationException("Invalid payment");
        
        if (!rules.CheckConditions(conditions, resolutionContext))
            throw new InvalidOperationException("Invalid conditions");
        
        if (!actionBlock.CheckTargets(resolutionContext))
            throw new InvalidOperationException("Invalid targets");

        return resolutionContext;
    }

    public static EffectPayload BindPayload(this IKeywordInstance effectInstance, IResolutionContext context)
    {
        return new EffectPayload(
            effectInstance.Id,
            context.Source, 
            effectInstance.Keyword, 
            effectInstance.Operands.Select(o => o.GetValue(context)).ToList()
        );
    }

    public static bool CheckTargets(this IActionBlock actionBlock, IResolutionContext context)
    {
        var targetDescriptors = actionBlock?.TargetsDescriptors ?? new List<IKeywordInstance>();
        var targets = context?.Targets ?? new List<IAtom>();

        if (targets.Count > targetDescriptors.Count 
            ||
            targets.Count < targetDescriptors.Count(CheckOptionality))
            throw new InvalidOperationException($"Invalid number of targets ({targets.Count})");

        foreach (var (description, target) in targetDescriptors.Zip(targets))
        {
            if (!FilterAtom(target, description))
                throw new InvalidOperationException($"Target ({target.Id}) does not match the description");
        }

        return true;
        
        bool CheckOptionality(IKeywordInstance keywordInstance)
        {
            var rules = context!.MetaGameState.Rules;
            if (rules.GetDefinition(keywordInstance.Keyword) is not TargetSpecificationDefinition definition)
                throw new InvalidOperationException($"Invalid target filter definition ({keywordInstance.Keyword})");
            
            return definition.IsOptional(context, keywordInstance);
        }
        
        bool FilterAtom(IAtom atom, IKeywordInstance keywordInstance)
        {
            var rules = context!.MetaGameState.Rules;
            if (rules.GetDefinition(keywordInstance.Keyword) is not TargetSpecificationDefinition definition)
                throw new InvalidOperationException($"Invalid target filter definition ({keywordInstance.Keyword})");
            
            return definition.Filter(atom, context, keywordInstance);
        }
    }
    
    
}