using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime.Actions;
using Archetype.Framework.Runtime.State;
using KeywordOperand = Archetype.Framework.Proto.KeywordOperand;

namespace Archetype.Framework.Runtime;

public static class RuntimeExtensions
{
    public static TDef GetOrThrow<TDef>(this IDefinitions definitions, KeywordInstance keywordInstance) where TDef : KeywordDefinition
    {
        return definitions.GetOrThrow<TDef>(keywordInstance.Keyword);
    }
    
    public static TDef GetOrThrow<TDef>(this IDefinitions definitions, string keyword) where TDef : KeywordDefinition
    {
        if (definitions.GetKeyword(keyword) is not TDef requiredDefinition)
            throw new InvalidOperationException($"Keyword ({keyword}) is not a {typeof(TDef).Name}");
        
        return requiredDefinition;
    }
    public static IEnumerable<(CostDefinition, CostPayload)> EnumerateCosts(this IDefinitions definitions, IEnumerable<CostInstance> costs, IEnumerable<CostPayload> payloads)
    {
        return costs.Select(definitions.GetOrThrow<CostDefinition>).Zip(payloads);
    }
    public static bool CheckCosts(this IDefinitions definitions, 
        IReadOnlyList<CostInstance> costs,
        IReadOnlyList<CostPayload> payments
        )
    {
        var cardsInPayment = payments.SelectMany(p => p.Payment).ToList();

        if (cardsInPayment.DistinctBy(c => c.Id).Count() != cardsInPayment.Count)
            throw new InvalidOperationException("Duplicate cards in payment");

        foreach (var (cost, payment) in costs.Zip(payments))
        {
            var costDefinition = definitions.GetOrThrow<CostDefinition>(cost);
            
            if (costDefinition.Type != payment.Type)
                throw new InvalidOperationException($"Cost type ({costDefinition.Type}) does not match payment type ({payment.Type})");
            
            if (!costDefinition.Check(payment, cost.Amount))
            {
                return false;
            }
        }
        
        return true;
    }
    
    public static T GetAtom<T> (this IGameState gameState, Guid id) where T : IAtom
    {
        if (!gameState.Atoms.TryGetValue(id, out var atom))
            throw new InvalidOperationException($"Atom ({id}) not found");

        if (atom is T requiredAtom)
            return requiredAtom;
        
        throw new InvalidOperationException($"Atom ({id}) is not a {typeof(T).Name}");
    }
    
    public static IAtom GetAtom(this IGameState gameState, Guid id)
    {
        if (gameState.Atoms.TryGetValue(id, out var atom))
            return atom;
        
        throw new InvalidOperationException($"Atom ({id}) not found");
    }
    
    public static bool CheckConditions(this IDefinitions definitions, IReadOnlyList<ConditionInstance> conditions, IAtom source, IGameState gameState)
    {
        return !conditions.Select(definitions.GetOrThrow<ConditionDefinition>)
            .All(c => c.Check(source, gameState));
    }

    public static IResolutionContext CreateResolutionContext(this IActionBlock actionBlock, IGameState gameState, IReadOnlyList<CostPayload> payments, IReadOnlyList<IAtom> targets)
    {
        return new ResolutionContext
        {
            GameState = gameState,
            Costs = payments,
            Source = actionBlock.Source,
            Targets = targets,
            ComputedValues = actionBlock.ComputedValues,
        };
    }
    
    public static IEnumerable<EffectInstance> GetPrimitives(this EffectInstance effectInstance, IDefinitions definitions, IResolutionContext context)
    {
        return definitions.GetKeyword(effectInstance.Keyword) switch
        {
            EffectCompositeDefinition composite => composite.CreateEffectSequence(context, definitions).SelectMany(e => e.GetPrimitives(definitions, context)),
            EffectPrimitiveDefinition primitive => new[] { effectInstance },
            _ => throw new InvalidOperationException($"Keyword ({effectInstance.Keyword}) is not an effect")
        };
    }

    public static Effect CreateEffect(this EffectInstance effectInstance, IResolutionContext context)
    {
        return new Effect
        {
            Source = context.Source,
            Keyword = effectInstance.Keyword,
            Operands = effectInstance.Operands.Select(o => o.GetValue(context)).ToList(),
            Targets = effectInstance.Targets.Select(t => t.GetTarget(context)).ToList()
        };
    }

    public static bool CheckTargets(this IActionBlock actionBlock, IReadOnlyList<IAtom> targets)
    {
        var targetDescriptors = actionBlock.TargetsDescriptors;

        if (targets.Count > targetDescriptors.Count 
            ||
            targets.Count < targetDescriptors.Count(d => !d.IsOptional))
            throw new InvalidOperationException($"Invalid number of targets ({targets.Count})");

        foreach (var (description, target) in targetDescriptors.Zip(targets))
        {
            if (description.CheckTarget(target))
                throw new InvalidOperationException($"Target ({target.Id}) does not match the description");
        }

        return true;
    }
    
    public static bool CheckTarget(this TargetDescription description, IAtom target)
    {
        foreach (var (keyword, requiredMatches) in description.Characteristics)
        {
            var characteristics = requiredMatches.Split('|').Select(s => s.Trim());
            
            if (!target.Characteristics.TryGetValue(keyword, out var value))
                return false;
  
            if (characteristics.Any(c => c != value))
                return false;
        }

        return true;
    }
}