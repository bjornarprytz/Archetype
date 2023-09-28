using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime.Actions;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Runtime;

public static class RuntimeExtensions
{
    public static TDef GetOrThrow<TDef>(this IDefinitions definitions, KeywordInstance keywordInstance) where TDef : KeywordDefinition
    {
        return definitions.GetOrThrow<TDef>(keywordInstance.Keyword);
    }
    
    public static TDef GetOrThrow<TDef>(this IDefinitions definitions, string keyword) where TDef : KeywordDefinition
    {
        if (definitions.GetDefinition(keyword) is not TDef requiredDefinition)
            throw new InvalidOperationException($"Keyword ({keyword}) is not a {typeof(TDef).Name}");
        
        return requiredDefinition;
    }
    public static IEnumerable<(CostDefinition, CostPayload, KeywordInstance)> EnumerateCosts(this IDefinitions definitions, IEnumerable<KeywordInstance> costs, IEnumerable<CostPayload> payloads)
    {
        var list = costs.ToList();
        
        return list.Select(definitions.GetOrThrow<CostDefinition>).Zip(payloads, list);
    }
    public static bool CheckCosts(this IDefinitions definitions, 
        IReadOnlyList<KeywordInstance> costs,
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
            
            if (!costDefinition.Check(payment, cost))
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
    
    public static bool CheckConditions(this IDefinitions definitions, IReadOnlyList<KeywordInstance> conditions, IAtom source, IGameState gameState)
    {
        return !conditions.Select(definitions.GetOrThrow<ConditionDefinition>)
            .All(c => c.Check(source, gameState));
    }

    public static IResolutionContext CreateResolutionContext(this IActionBlock actionBlock, IGameRoot gameRoot, IReadOnlyList<CostPayload> payments, IReadOnlyList<IAtom> targets)
    {
        var definitions = gameRoot.MetaGameState.Definitions;
        var gameState = gameRoot.GameState;
        var conditions = actionBlock.Conditions;
        var costs = actionBlock.Costs;
        var source = actionBlock.Source;

        actionBlock.UpdateComputedValues(definitions, gameState);
        
        var resolutionContext = new ResolutionContext
        {
            MetaGameState = gameRoot.MetaGameState,
            GameState = gameRoot.GameState,
            Costs = payments,
            Source = actionBlock.Source,
            Targets = targets,
            ComputedValues = actionBlock.ComputedValues,
        };

        if (definitions.CheckConditions(conditions, source, gameState))
            throw new InvalidOperationException("Invalid conditions");
        
        if (!definitions.CheckCosts(costs, payments))
            throw new InvalidOperationException("Invalid payment");
        
        if (actionBlock.CheckTargets(resolutionContext))
            throw new InvalidOperationException("Invalid targets");

        return resolutionContext;
    }
    
    public static IEnumerable<KeywordInstance> GetPrimitives(this KeywordInstance effectInstance, IDefinitions definitions, IResolutionContext context)
    {
        return definitions.GetDefinition(effectInstance.Keyword) switch
        {
            EffectCompositeDefinition composite => composite.CreateEffectSequence(context, definitions).SelectMany(e => e.GetPrimitives(definitions, context)),
            EffectPrimitiveDefinition primitive => new[] { effectInstance },
            _ => throw new InvalidOperationException($"Keyword ({effectInstance.Keyword}) is not an effect")
        };
    }

    public static Effect BindPayload(this KeywordInstance effectInstance, IResolutionContext context)
    {
        return new Effect(
            context.Source, 
            effectInstance.Keyword, 
            effectInstance.Operands.Select(o => o.GetValue(context)).ToList(), 
            effectInstance.Targets.Select(t => t.GetTarget(context)).ToList()
        );
    }

    public static bool CheckTargets(this IActionBlock actionBlock, IResolutionContext context)
    {
        var targetDescriptors = actionBlock.TargetsDescriptors;
        var targets = context.Targets;

        if (targets.Count > targetDescriptors.Count 
            ||
            targets.Count < targetDescriptors.Count(d => !d.IsOptional))
            throw new InvalidOperationException($"Invalid number of targets ({targets.Count})");

        foreach (var (description, target) in targetDescriptors.Zip(targets))
        {
            if (description.Filter.FilterAtom(target, context))
                throw new InvalidOperationException($"Target ({target.Id}) does not match the description");
        }

        return true;
    }
    
    public static bool HasCharacteristic/*<T>*/(this IAtom atom, string key, string stringValue, IResolutionContext? context=null)
    {
        return atom.Characteristics.TryGetValue(key, out var instance) 
               // && instance is CharacteristicInstance<T> { TypedValue: { } typedValue } 
               && (stringValue == "any" || instance.Operands[0].GetValue(context).Equals(stringValue));
    }
}