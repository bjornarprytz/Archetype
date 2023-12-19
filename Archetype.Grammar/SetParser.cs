using System.Text.Json;
using Archetype.Framework.DependencyInjection;
using Archetype.Framework.Design;

namespace Archetype.Grammar;

public record SetData(string Name, string Description, IEnumerable<CardData> Cards);


public class SetParser(IRules rules) : ISetParser
{
    public IEnumerable<IProtoSet> ParseSets(string setData)
    {
        var cardParser = new CardParser();
        var sets = JsonSerializer.Deserialize<SetData[]>(setData);
        
        if (sets is null)
        {
            throw new Exception($"Failed to deserialize set data ({setData}).");
        }
        
        var protoSets = new List<IProtoSet>();
        
        foreach (var set in sets)
        {
            var protoSet = new ProtoSet(set.Name, set.Description);
            protoSet.MutableCards.AddRange(set.Cards.Select(cardParser.ParseCard));
            protoSets.Add(protoSet);
        }

        return protoSets;
    }
    
    private record ProtoSet(string Name, string Description) : IProtoSet
    {
        public List<IProtoCard> MutableCards { get; } = new();
        
        public IReadOnlyList<IProtoCard> Cards => MutableCards;
    }
}