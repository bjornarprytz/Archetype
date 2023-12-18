using Archetype.Framework.DependencyInjection;
using Archetype.Framework.Design;

namespace Archetype.Grammar;

public record CardData(string RulesText, string ArtLink);

public class CardParser(IProtoCardBuilderFactory builderFactory)
{
    BaseGrammarParser _grammarParser = new();
    
    public IProtoCard ParseCard(CardData cardData)
    {
        var builder = builderFactory.CreateBuilder();

        var parser = Helper.CreateParser(cardData.RulesText);
        
        parser.cardText() // TODO: Continue here
        
        var artLink = cardData.ArtLink;
        
        var artLinkParser = new ArtLinkParser(artLink, builder);
        artLinkParser.ParseArtLink();
        
        return builder.Build();
    }
}