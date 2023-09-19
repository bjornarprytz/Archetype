using System.Text.Json;
using Antlr4.Runtime;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;

namespace Archetype.Framework.Parsing;

public class SetData
{
    public string Name { get; set; }
    public string Description { get; set; }
    public IEnumerable<CardData> Cards { get; set; }
}

public class CardData
{
    public string Name { get; set; }
    public string Text { get; set; }
}

public interface ICardParser
{
    public ProtoCard ParseCard(CardData cardData);
}

public interface ISetParser
{
    public ProtoSet ParseSet(string setJson);
}

public class Parser : ISetParser
{
    private readonly ICardParser _cardParser;

    public Parser(ICardParser cardParser)
    {
        _cardParser = cardParser;
    }

    public ProtoSet ParseSet(string setJson)
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
    private readonly IDefinitions _definitions;

    public CardParser(IDefinitions definitions)
    {
        _definitions = definitions;
    }

    public ProtoCard ParseCard(CardData cardData)
    {
        var inputStream = new AntlrInputStream(cardData.Text);
        var lexer = new ActionBlockLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new ActionBlockParser(tokenStream);
        
        var tree = parser.actionBlock();
        

        var t = tree.ToStringTree(parser);

        Console.WriteLine(t);

        return null;
    }
}