using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Design;
using Archetype.Framework.Interface.Actions;

namespace Archetype.Framework.Extensions;

public static class RulesExtensions
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
    
    public static bool CheckConditions(this IRules rules, IReadOnlyList<IKeywordInstance> conditions, IResolutionContext context)
    {
        return conditions.All(keywordInstance => rules.GetOrThrow<ConditionDefinition>(keywordInstance).Check(context, keywordInstance));
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
}