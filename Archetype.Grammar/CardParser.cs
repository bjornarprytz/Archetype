using Archetype.Framework.DependencyInjection;
using Archetype.Framework.Design;

namespace Archetype.Grammar;

public record CardData(string RulesText, string ArtLink);

public class CardParser()
{
    public IProtoCard ParseCard(CardData cardData)
    {
        var builder = new ProtoCardBuilder();

        var parser = Helper.CreateParser(cardData.RulesText);

        var cardTextContext = parser.cardText(); // TODO: Continue here
        
        return builder.Build();
    }
}