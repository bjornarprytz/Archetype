using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime.Actions;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Runtime;

public static class RuntimeExtensions
{
    public static TDef GetOrThrow<TDef>(this IDefinitions definitions, KeywordInstance keywordInstance) where TDef : KeywordDefinition
    {
        if (definitions.Keywords[keywordInstance.Keyword] is not TDef requiredDefinition)
            throw new InvalidOperationException($"Keyword ({keywordInstance.Keyword}) is not a {typeof(TDef).Name}");
        
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
            Effects = actionBlock.Effects.Select(effectInstance => effectInstance.CreateEffect(actionBlock, targets)).ToList(),
            Costs = payments,
            Source = actionBlock.Source,
            Targets = targets,
        };

    }

    public static Effect CreateEffect(this EffectInstance effectInstance, IActionBlock actionBlock, IReadOnlyList<IAtom> targets)
    {
        return new Effect
        {
            Source = actionBlock.Source,
            Keyword = effectInstance.Keyword,
            Operands = effectInstance.Operands.GetOperands(actionBlock),
            Targets = effectInstance.Targets.GetTargets(targets)
        };
    }
    
    public static IReadOnlyList<object> GetOperands(this IReadOnlyList<OperandDescription> operandDescriptions,
        IActionBlock actionBlock)
    {
        var operands = new List<object>();

        foreach (var operand in operandDescriptions)
        {
            if (operand.IsComputed)
            {
                if (actionBlock.GetComputedValue(operand.ComputedPropertyKey) is {} computedValue)
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

    public static bool CheckTargets(this IActionBlock actionBlock, IReadOnlyList<IAtom> targets)
    {
        var targetDescriptors = actionBlock.GetTargetDescriptors().ToList();

        if (targets.Count > targetDescriptors.Count 
            ||
            targets.Count < targetDescriptors.Count(d => !d.IsOptional))
            throw new InvalidOperationException($"Invalid number of targets ({targets.Count})");

        foreach (var (description, target) in targetDescriptors.Zip(targets))
        {
            if (description.CheckTarget(target))
                throw new InvalidOperationException($"Target ({target.Id}) does not match type the description ({description.CharacteristicsMatch})");
        }

        return true;
    }
    
    public static IReadOnlyDictionary<int, IAtom> GetTargets(this IReadOnlyList<TargetDescription> targetDescriptions,
        IReadOnlyList<IAtom> targets)
    {
        var result = new Dictionary<int, IAtom>();
        
        foreach (var targetDescription in targetDescriptions.DistinctBy(t => t.Index))
        {
            if (targetDescription.Index >= targets.Count)
                throw new InvalidOperationException($"Target index ({targetDescription.Index}) is out of range");
            if (!targetDescription.IsOptional && !targetDescription.CheckTarget(targets[targetDescription.Index]))
                throw new InvalidOperationException($"Target ({targets[targetDescription.Index].Id}) does not match type the description ({targetDescription.CharacteristicsMatch})");
            
            result.Add(targetDescription.Index, targets[targetDescription.Index]);
        }
        
        return result;
    }
    public static bool CheckTarget(this TargetDescription targetDescription, IAtom target)
    {
        return 
            targetDescription.CharacteristicsMatch.Values.All(c => 
                    target.Characteristics[c].Contains(
                        targetDescription.CharacteristicsMatch[c]
                        )
                    );
    }
}