using System.Text.Json;
using Antlr4.Runtime;
using Archetype.Framework.DependencyInjection;
using Archetype.Framework.Design;
using FluentValidation;

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

public class CardParser(IRules rules) : ICardParser
{
    public IProtoCard ParseCard(CardData cardData)
    {
        var inputStream = new AntlrInputStream(cardData.Text);
        var lexer = new ActionBlockLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new ActionBlockParser(tokenStream);
        
        var tree = parser.cardText();

        var protoBuilder = new ProtoCardBuilder(Enumerable.Empty<IValidator<IProtoCard>>(), Enumerable.Empty<Action<ProtoCardBuilder.ProtoCard>>());

        var name = tree.name().STRING().GetText().Trim('"');
        protoBuilder.SetName(name);
        
        var characteristics = tree.@static().keywordExpression().Select(kw => kw.GetKeywordInstance(rules)).ToList();

        protoBuilder.AddCharacteristics(characteristics);

        var costs = tree.effects().actionBlock().GetCosts(rules).ToList();
        var conditions = tree.effects().actionBlock().GetConditions(rules).ToList();
        var targetSpecs = tree.effects().actionBlock().GetTargetSpecs().ToList();
        var computedValues = tree.effects().actionBlock().GetComputedValues(rules).ToList();
        var effects = tree.effects().actionBlock().GetEffectKeywordInstances(rules).ToList();
        
        protoBuilder.SetActionBlock(targetSpecs, costs, conditions, computedValues, effects);
        
        return protoBuilder.Build();
    }
}


























