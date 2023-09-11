using System.Collections.ObjectModel;
using System.Text.Json;
using Archetype.Core;
using Archetype.Rules;
using Archetype.Rules.Proto;
using Archetype.Rules.State;

namespace Archetype.Parsing;

public class SetData
{
    public string Name { get; set; }
    public string Description { get; set; }
    public IEnumerable<CardData> Cards { get; set; }
}

public class CardData
{
    public string Name { get; set; }
    public CardType Type { get; set; }
    public string Text { get; set; }
    public Dictionary<string, string> Characteristics { get; set; }
}

public interface ICardParser
{
    public ProtoCard ParseCard(CardData cardData);
}

public interface ISetParser
{
    public ProtoSet ParseGame(string setJson);
}

public class Parser : ISetParser
{
    private readonly ICardParser _cardParser;

    public Parser(ICardParser cardParser)
    {
        _cardParser = cardParser;
    }

    public ProtoSet ParseGame(string setJson)
    {
        var setData = JsonSerializer.Deserialize<SetData>(setJson);

        if (setData is null)
            throw new InvalidOperationException("Could not parse set data");
        
        var cards = setData.Cards.Select(_cardParser.ParseCard);
        
        return new ProtoSet
        {
            Name = setData.Name,
            Description = setData.Description,
            Cards = cards.ToList()
        };
    }
}

public class CardParser : ICardParser
{
    private readonly Definitions _definitions;

    public CardParser(Definitions definitions)
    {
        _definitions = definitions;
    }

    public ProtoCard ParseCard(CardData cardData)
    {
        var cost = new List<CostInstance>();
        var conditions = new List<ConditionInstance>();
        var reactions = new List<ReactionInstance>();
        var effects = new List<EffectInstance>();
        var features = new List<FeatureInstance>();
        var abilities = new List<AbilityInstance>();
        var computedProperties = new List<ComputedPropertyInstance>();

        foreach (var token in cardData.Text.Split(";"))
        {
            var definition = _definitions.Keywords.Values.FirstOrDefault(def => def.Pattern.Match(token).Success);
            
            if (definition is null)
                throw new InvalidOperationException($"Could not parse token: {token}");

            var protoData = definition.Parse(token);

            switch (protoData)
            {
                case CostInstance costInstance:
                    cost.Add(costInstance);
                    break;
                case ConditionInstance conditionInstance:
                    conditions.Add(conditionInstance);
                    break;
                case ReactionInstance reactionInstance:
                    reactions.Add(reactionInstance);
                    break;
                case EffectInstance effectInstance:
                    effects.Add(effectInstance);
                    break;
                case FeatureInstance featureInstance:
                    features.Add(featureInstance);
                    break;
                case AbilityInstance abilityInstance:
                    abilities.Add(abilityInstance);
                    break;
                case ComputedPropertyInstance computedPropertyInstance:
                    computedProperties.Add(computedPropertyInstance);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown keyword instance type: {protoData.GetType().Name}");
            }
        }
        
        
        var protoCard = new ProtoCard
        {
            Name = cardData.Name,
            Type = cardData.Type,
            
            Characteristics = new ReadOnlyDictionary<string, string>(cardData.Characteristics)
        };
        
        return protoCard;
    }
}