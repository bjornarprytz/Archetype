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

        var cardTextContext = parser.cardText();
        
        builder.SetName(cardTextContext.name().STRING().ToString()!);
        builder.AddCharacteristics(cardTextContext.@static().staticKeyword().Select((staticKwContext =>
        {
            staticKwContext. // TOOD: Characteristic syntax (To strive for "TYPE: Creature" instead of "TYPE(Creature)" or Characteristic("TYPE", "Creature"))
        })));
        
        
        return builder.Build();
    }
}