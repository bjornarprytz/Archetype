using System.Text.Json;
using Archetype.Framework.Design;

namespace Archetype.Framework.Parsing;

public class SetData
{
    public string Name { get; set; }
    public string Description { get; set; }
    public IEnumerable<CardData> Cards { get; set; }
}

public class CardData
{
    public string Text { get; set; }
}

public interface ICardParser
{
    public IProtoCard ParseCard(CardData cardData);
}

public interface ISetParser
{
    public IProtoSet ParseSet(string setJson);
}

public class SetParser(ICardParser cardParser) : ISetParser
{
    public IProtoSet ParseSet(string setJson)
    {
        var setData = JsonSerializer.Deserialize<SetData>(setJson);

        if (setData is null)
            throw new InvalidOperationException("Could not parse set data");
        
        var cards = setData.Cards.Select(cardParser.ParseCard);
        
        return new ProtoSet
        (
            setData.Name,
            setData.Description,
            cards.ToList()
        );
    }

    private record ProtoSet(string Name, string Description, IReadOnlyList<IProtoCard> Cards) : IProtoSet;
}


























