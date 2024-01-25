using Archetype.Framework.BaseRules.Keywords;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.Interface.Actions;
using Archetype.Framework.Meta;

namespace Archetype.Framework.BaseRules;

public static class Cost
{
    [Cost("COST")]
    public static IEffectResult PayResources(IResolutionContext context, PaymentPayload payment)
    {
        if (payment.Payment.DistinctBy(a => a.Id).Count() != payment.Payment.Count)
            return EffectResult.Failed;
        
        var requiredAmount = payment.Amount;
        
        var paidAmount = payment.Payment.Sum(a => a.GetStatValue("VALUE"));
        
        if (paidAmount < requiredAmount)
        {
            return EffectResult.Failed;
        }

        var discardPile = context.GameState.Player.DiscardPile;

        return KeywordFrame.Compose(
            payment.Payment.Select(a => Instance.BindArgs(Effects.ChangeZone, a, discardPile)).ToArray()
            );
    }
}