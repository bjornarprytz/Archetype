using System.Reflection;
using Archetype.Framework.Core;
using Archetype.Framework.Data;
using Archetype.Framework.Effects;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework.Parsing;

public class ExpressionParser
{
    private readonly Dictionary<string, MethodInfo> _effectMethods;

    public ExpressionParser(IEnumerable<MethodInfo> effectMethods)
    {
        _effectMethods = effectMethods.ToDictionary(m => m.Name);
    }
    
    public CardProto ParseCard(CardData cardData)
    {
        return new CardProto()
        {
            Name = cardData.Name,
            Costs = ParseCost(cardData.Costs),
            Targets = cardData.Targets.Select(ParseTargets).ToArray(),
            
            Stats = cardData.Stats.ToDictionary(kvp => kvp.Key, kvp => ParseStat(kvp.Value)),
            Facets = cardData.Facets,
            Tags = cardData.Tags.ToHashSet(),
            
            Effects = cardData.Effects.Select(ParseEffect).ToArray(),
            Variables = cardData.Variables.ToDictionary(kvp => kvp.Key, kvp => ParseVariable(kvp.Value)),
        };
    }
    
    private TargetDescriptor ParseTargets(TargetData targetData)
    {
        throw new NotImplementedException();
    }

    private CostResolver ParseCost(Dictionary<string, Expression> expression)
    {
        throw new NotImplementedException();
    }

    private StatResolver ParseStat(Expression expression)
    {
        
        throw new NotImplementedException();
    }

    private VariableResolver ParseVariable(Expression expression)
    {
        // TODO: Walk the expression and extract the value from the input object
        
        throw new NotImplementedException();
    }

    private EffectResolver ParseEffect(EffectData effectData)
    {
        throw new NotImplementedException();
    }
}