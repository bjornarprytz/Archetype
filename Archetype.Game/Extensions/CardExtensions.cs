using System.Linq;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.Game.Extensions
{
    public static class CardExtensions
    {
        public static bool ValidateTargets(this ICard card, ICardResolutionContext context)
        {
            var targetCount = card.Targets.Count();

            if (context.Targets.Count() != targetCount)
                return false;
            
            foreach (var (targetData, chosenTarget) in card.Targets.Zip(context.Targets))
            {
                if (!targetData.ValidateContext(new TargetValidationContext(context.GameState, chosenTarget)))
                    return false;
            }

            return true;
        }

        public static void Resolve(this ICard card, ICardResolutionContext context)
        {
            foreach (var effect in card.Effects)
            {
                effect.ResolveContext(context);
            }
        }
    }
}