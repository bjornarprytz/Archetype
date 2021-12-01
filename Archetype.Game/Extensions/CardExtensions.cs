using System.Linq;
using Archetype.Game.Exceptions;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.PlayContext;

namespace Archetype.Game.Extensions
{
    public static class CardExtensions
    {
        public static bool ValidateTargets(this ICard card, ICardResolutionContext context)
        {
            var targetCount = card.Targets.Count();

            if (context.Targets.Count() != targetCount)
            {
                throw new TargetCountMismatchException(targetCount, context.Targets.Count());
            }
            
            foreach (var (targetData, chosenTarget) in card.Targets.Zip(context.Targets))
            {
                if (!targetData.ValidateContext(new TargetValidationContext(context.GameState, chosenTarget)))
                {
                    throw new InvalidTargetChosenException();
                }
            }

            return true;
        }
    }
}