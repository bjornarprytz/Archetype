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
        
        
        // TODO: Refactor this repeated code
        builder.AddStats(characteristics);
        builder.BuildActionBlock(
            actionBlockBuilder =>
            {
                actionBlockBuilder.AddTargetSpecs(cardTextContext.actionBlock().GetTargetSpecsContext().ParseFunctionLike());
                actionBlockBuilder.AddCosts(cardTextContext.actionBlock().GetCostContext().ParseFunctionLike());
                actionBlockBuilder.AddConditions(cardTextContext.actionBlock().GetConditionContext().ParseFunctionLike());
                actionBlockBuilder.AddComputedValues(cardTextContext.actionBlock().GetComputedValueContext().ParseFunctionLike());
                actionBlockBuilder.AddEffects(cardTextContext.actionBlock().GetEffectContext().ParseFunctionLike());
            }
        );

        foreach (var abilityContext in cardTextContext.GetAbilitiesContext())
        {
            var name = abilityContext.name().STRING().ToString()!.TrimSTRING();
            
            builder.AddAbility(
                name,
                actionBlockBuilder =>
                {
                    actionBlockBuilder.AddTargetSpecs(abilityContext.actionBlock().GetTargetSpecsContext().ParseFunctionLike());
                    actionBlockBuilder.AddCosts(abilityContext.actionBlock().GetCostContext().ParseFunctionLike());
                    actionBlockBuilder.AddConditions(abilityContext.actionBlock().GetConditionContext().ParseFunctionLike());
                    actionBlockBuilder.AddComputedValues(abilityContext.actionBlock().GetComputedValueContext().ParseFunctionLike());
                    actionBlockBuilder.AddEffects(abilityContext.actionBlock().GetEffectContext().ParseFunctionLike());
                }
            );

        }

        
        return builder.Build();
    }
    
    
    
}