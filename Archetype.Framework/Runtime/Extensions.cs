using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime.Actions;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Runtime;

public static class RuntimeExtensions
{
    public static TDef GetOrThrow<TDef>(this IRules rules, IKeywordInstance keywordInstance) where TDef : IKeywordDefinition
    {
        return rules.GetOrThrow<TDef>(keywordInstance.Keyword);
    }
    
    public static IKeywordDefinition? GetOrThrow(this IRules rules, IKeywordInstance keywordInstance)
    {
        return rules.GetDefinition(keywordInstance.Keyword) ?? throw new InvalidOperationException($"Keyword ({keywordInstance.Keyword}) not found");
    }
    
    public static TDef GetOrThrow<TDef>(this IRules rules, string keyword) where TDef : IKeywordDefinition
    {
        if (rules.GetDefinition(keyword) is not TDef requiredDefinition)
            throw new InvalidOperationException($"Keyword ({keyword}) is not a {typeof(TDef).Name}");
        
        return requiredDefinition;
    }
    
    public static TDef GetOrThrow<TDef>(this IRules rules) where TDef : IKeywordDefinition
    {
        if (rules.GetDefinition<TDef>() is not { } requiredDefinition)
            throw new InvalidOperationException($"There is no definition for {typeof(TDef).Name}");
        
        return requiredDefinition;
    }
    public static IEnumerable<(CostDefinition, PaymentPayload, IKeywordInstance)> EnumerateCosts(this IRules rules, IEnumerable<IKeywordInstance> costs, IEnumerable<PaymentPayload> payloads)
    {
        var list = costs.ToList();
        
        return list.Select(rules.GetOrThrow<CostDefinition>).Zip(payloads, list);
    }
    public static bool CheckPayments(this IRules rules, 
        IResolutionContext context,
        IReadOnlyList<IKeywordInstance> costs,
        IReadOnlyList<PaymentPayload> payments
        )
    {
        var cardsInPayment = payments.SelectMany(p => p.Payment).ToList();

        if (cardsInPayment.DistinctBy(c => c.Id).Count() != cardsInPayment.Count)
            throw new InvalidOperationException("Duplicate cards in payment");

        foreach (var (cost, payment) in costs.Zip(payments))
        {
            var costDefinition = rules.GetOrThrow<CostDefinition>(cost);
            
            if (costDefinition.Type != payment.Type)
                throw new InvalidOperationException($"Cost type ({costDefinition.Type}) does not match payment type ({payment.Type})");
            
            if (!costDefinition.Check(context, payment, cost))
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
    
    public static void AddAtom(this IGameState gameState, IAtom atom)
    {
        gameState.Atoms.Add(atom.Id, atom);
    }
    
    public static bool CheckConditions(this IRules rules, IReadOnlyList<IKeywordInstance> conditions, IAtom source, IGameState gameState)
    {
        return !conditions.Select(rules.GetOrThrow<ConditionDefinition>)
            .All(c => c.Check(source, gameState));
    }

    public static IResolutionContext CreateAndValidateResolutionContext(this IActionBlock actionBlock, IGameRoot gameRoot, IReadOnlyList<PaymentPayload> payments, IReadOnlyList<IAtom> targets)
    {
        var definitions = gameRoot.MetaGameState.Rules;
        var gameState = gameRoot.GameState;
        var conditions = actionBlock.Conditions;
        var costs = actionBlock.Costs;
        var source = actionBlock.Source;
        
        actionBlock.UpdateComputedValues(definitions, gameState);
        
        var resolutionContext = new ResolutionContext
        {
            MetaGameState = gameRoot.MetaGameState,
            GameState = gameRoot.GameState,
            Payments = payments.ToDictionary(k => k.Type, v => v),
            Source = actionBlock.Source,
            Targets = targets,
            ComputedValues = actionBlock.ComputedValues,
        };
        
        if (!definitions.CheckPayments(resolutionContext, costs, payments))
            throw new InvalidOperationException("Invalid payment");
        
        if (definitions.CheckConditions(conditions, source, gameState))
            throw new InvalidOperationException("Invalid conditions");
        
        
        if (actionBlock.CheckTargets(resolutionContext))
            throw new InvalidOperationException("Invalid targets");

        return resolutionContext;
    }

    public static EffectPayload BindPayload(this IKeywordInstance effectInstance, IResolutionContext context)
    {
        return new EffectPayload(
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
    
    public static bool HasCharacteristic/*<T>*/(this IAtom atom, string key, string stringValue, IResolutionContext context)
    {
        return atom.Characteristics.TryGetValue(key, out var instance) 
               // && instance is CharacteristicInstance<T> { TypedValue: { } typedValue } 
               && (stringValue == "any" || instance.Operands[0].GetValue(context) is {} v && v.Equals(stringValue));
    }
    
    public static T? GetState<T>(this IAtom atom, string key)
    {
        if (!atom.State.TryGetValue(key, out var value)) return default;
        
        if (value is T typedValue)
            return typedValue;
            
        throw new InvalidOperationException($"State key {key} is not of type {typeof(T).Name}");

    }
    
    public static IKeywordInstance? GetCharacteristic(this IAtom atom, string key)
    {
        return !atom.Characteristics.TryGetValue(key, out var value) ? null : value;
    }
    
    public static int GetCharacteristicValue(this IAtom atom, string key, IResolutionContext context)
    {
        var keywordInstance = atom.GetCharacteristic(key);
        if (keywordInstance is null)
            throw new InvalidOperationException($"Characteristic {key} not found");

        if (keywordInstance.Operands.Count == 0)
            throw new InvalidOperationException($"Characteristic {key} has no operands");
        
        return keywordInstance.Operands[0].GetValue(context) switch
        {
            int intValue => intValue,
            string stringValue when int.TryParse(stringValue, out var intValueFromString) => intValueFromString,
            _ => throw new InvalidOperationException($"Characteristic {key} is not of type integer, or cannot be parsed as an integer")
        };
    }
    
    public static int GetResourceValue(this IAtom atom)
    {
        var value = atom.GetCharacteristic("RESOURCE_VALUE")?.Operands[0].GetValue(null);

        return value switch
        {
            int intValue => intValue,
            string stringValue when int.TryParse(stringValue, out var intValueFromString) => intValueFromString,
            _ => 0
        };
    }
}