using Archetype.Rules.Definitions;
using Archetype.Rules.Proto;

namespace Archetype.Rules;

public static class DefinitionExtensions
{
    public static TDef GetOrThrow<TDef>(this State.Definitions definitions, ProtoData protoData) where TDef : KeywordDefinition
    {
        if (definitions.Keywords[protoData.Keyword] is not TDef requiredDefinition)
            throw new InvalidOperationException($"Keyword ({protoData.Keyword}) is not a {typeof(TDef).Name}");
        
        return requiredDefinition;
    }
    public static IEnumerable<(CostDefinition, CostPayload)> EnumerateCosts(this State.Definitions definitions, IEnumerable<ProtoCost> costs, IEnumerable<CostPayload> payloads)
    {
        return costs.Select(definitions.GetOrThrow<CostDefinition>).Zip(payloads);
    }
    public static bool CheckCosts(this State.Definitions definitions, 
        IReadOnlyList<ProtoCost> costs,
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
}