using Archetype.Framework.BaseRules.Keywords;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.Interface.Actions;
using Archetype.Framework.Meta;
using Archetype.Framework.State;

namespace Archetype.Framework.BaseRules;

[KeywordCollection]
public static class Cost
{
    [Cost( CostType.Resource, "COST")]
    public static IEffectResult PayResources(IResolutionContext context, int requiredAmount, List<IAtom> payment)
    {
        if (payment.DistinctBy(a => a.Id).Count() != payment.Count)
            return EffectResult.Failed("Duplicate atom in payment");
        
        var paidAmount = payment.Sum(a => a.GetStat("VALUE"));
        
        if (paidAmount < requiredAmount)
        {
            return EffectResult.Failed("Insufficient payment");
        }

        var discardPile = context.GameState.Player.DiscardPile;

        return KeywordFrame.Compose(
            payment.Select(a => Instance.BindArgs(Effects.ChangeZone, a, discardPile)).ToArray()
            );
    }
}