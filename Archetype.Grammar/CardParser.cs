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
        
        builder.SetName(cardTextContext.name().STRING().ToString()!);

        builder.AddCharacteristics(cardTextContext.@static().staticKeyword().Select(ParseExtensions.ParseStaticKeywordContext).ToList());
        builder.SetActionBlock(
    cardTextContext.effects().actionBlock().targets().targetSpecs().Select(ParseExtensions.ParseTargetSpecContext).ToList(),
            cardTextContext.effects().actionBlock().costKeyword().Select(ParseExtensions.ParseCostContext).ToList(),
    cardTextContext.effects().actionBlock().conditionKeyword().Select(ParseExtensions.ParseConditionContext).ToList(),
    cardTextContext.effects().actionBlock().computedValues().ParseComputedValueContext().ToList(),
    cardTextContext.effects().actionBlock().effect().Select(ParseExtensions.ParseEffectContext).ToList()
            );
        
        
        return builder.Build();
    }
    
    
    
}