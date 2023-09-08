using System.Collections.ObjectModel;
using System.Text.Json;

namespace Archetype.Framework;

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
        var cost = new List<ProtoCost>();
        var conditions = new List<ProtoCondition>();
        var reactions = new List<ProtoReaction>();
        var effects = new List<ProtoEffect>();
        var auras = new List<ProtoAura>();
        var features = new List<ProtoFeature>();
        var abilities = new List<ProtoAbility>();

        foreach (var token in cardData.Text.Split(";"))
        {
            var definition = _definitions.Keywords.Values.FirstOrDefault(def => def.Pattern.Match(token).Success);
            
            if (definition is null)
                throw new InvalidOperationException($"Could not parse token: {token}");

            var protoData = definition.Parse(token);

            switch (protoData)
            {
                case ProtoCost protoCost:
                    cost.Add(protoCost);
                    break;
                case ProtoCondition protoCondition:
                    conditions.Add(protoCondition);
                    break;
                case ProtoReaction protoReaction:
                    reactions.Add(protoReaction);
                    break;
                case ProtoEffect protoEffect:
                    effects.Add(protoEffect);
                    break;
                case ProtoAura protoAura:
                    auras.Add(protoAura);
                    break;
                case ProtoFeature protoFeature:
                    features.Add(protoFeature);
                    break;
                case ProtoAbility protoAbility:
                    abilities.Add(protoAbility);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown proto data type: {protoData.GetType().Name}");
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