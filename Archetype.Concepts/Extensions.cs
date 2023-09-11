using Archetype.Core;
using Archetype.Rules.Definitions;
using Archetype.Rules.Proto;
using Archetype.Rules.State;

namespace Archetype.Rules;

public static class DefinitionExtensions
{
    public static TDef GetOrThrow<TDef>(this State.Definitions definitions, KeywordInstance keywordInstance) where TDef : KeywordDefinition
    {
        if (definitions.Keywords[keywordInstance.Keyword] is not TDef requiredDefinition)
            throw new InvalidOperationException($"Keyword ({keywordInstance.Keyword}) is not a {typeof(TDef).Name}");
        
        return requiredDefinition;
    }
    public static IEnumerable<(CostDefinition, CostPayload)> EnumerateCosts(this State.Definitions definitions, IEnumerable<CostInstance> costs, IEnumerable<CostPayload> payloads)
    {
        return costs.Select(definitions.GetOrThrow<CostDefinition>).Zip(payloads);
    }
    public static bool CheckCosts(this State.Definitions definitions, 
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
    
    public static bool CheckConditions(this State.Definitions definitions, IReadOnlyList<ConditionInstance> conditions, Card source, GameState gameState)
    {
        return !conditions.Select(definitions.GetOrThrow<ConditionDefinition>)
            .All(c => c.Check(source, gameState));
    }

    public static IEnumerable<Effect> CreateEffects(this IReadOnlyList<EffectInstance> effectInstances, Card source, IComputedValuesCache computedValuesCache, IReadOnlyList<Card> targets)
    {
        return effectInstances.Select(effectInstance => new Effect
        {
            Keyword = effectInstance.Keyword,
            Operands = effectInstance.Operands.GetOperands(computedValuesCache),
            Source = source,
            Targets = effectInstance.Targets.GetTargets(targets)
        });
    }

    public static IReadOnlyList<object> GetOperands(this IReadOnlyList<OperandDescription> operandDescriptions,
        IComputedValuesCache computedValuesCache)
    {
        var operands = new List<object>();

        foreach (var operand in operandDescriptions)
        {
            if (operand.IsComputed)
            {
                if (computedValuesCache.GetComputedValue(operand.ComputedPropertyKey) is {} computedValue)
                    operands.Add(computedValue);
                else
                {
                    throw new InvalidOperationException($"Computed property ({operand.ComputedPropertyKey}) not found");
                }
            }
            else
            {
                operands.Add(operand.Value);
            }
        }

        return operands;
    }

    public static IReadOnlyDictionary<int, Card> GetTargets(this IReadOnlyList<TargetDescription> targetDescriptions,
        IReadOnlyList<Card> targets)
    {
        var result = new Dictionary<int, Card>();
        
        foreach (var targetDescription in targetDescriptions.DistinctBy(t => t.Index))
        {
            if (targetDescription.Index >= targets.Count)
                throw new InvalidOperationException($"Target index ({targetDescription.Index}) is out of range");
            if (!targetDescription.IsOptional && !targetDescription.CheckTarget(targets[targetDescription.Index]))
                throw new InvalidOperationException($"Target ({targets[targetDescription.Index].Id}) does not match type ({targetDescription.Type})");
            
            result.Add(targetDescription.Index, targets[targetDescription.Index]);
        }
        
        return result;
    }
    public static bool CheckTarget(this TargetDescription targetDescription, Card target)
    {
        return targetDescription.Type.HasFlag(target.Proto.Type);
    }
}