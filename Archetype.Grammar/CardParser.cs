using Archetype.Framework.Core.Primitives;
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
        
        builder.SetName(cardTextContext.name().STRING().ToString()!.TrimSTRING());

        var characteristics = cardTextContext.@static().Select(ParseExtensions.ParseStaticKeywordContext).ToList();

        var effects = cardTextContext.actionBlock().effect();
        
        builder.AddCharacteristics(characteristics);
        builder.SetActionBlock(
    cardTextContext.actionBlock().targets().targetSpecs().Select(ParseExtensions.ParseTargetSpecContext).ToList(),
            cardTextContext.actionBlock().cost().Select(ParseExtensions.ParseCostContext).ToList(),
    cardTextContext.actionBlock().condition().Select(ParseExtensions.ParseConditionContext).ToList(),
    cardTextContext.actionBlock().computedValues().ParseComputedValueContext().ToList(),
    cardTextContext.actionBlock().effect().Select(ParseExtensions.ParseEffectContext).ToList()
            );
        
        
        return builder.Build();
    }
    
    
    
}