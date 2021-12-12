using System.Linq;
using Archetype.Game.Exceptions;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Extensions
{
    public static class CardExtensions
    {
        public static void ValidateTargets(this ICardPlayArgs cardArgs, IGameState gameState)
        {
            var requiredTargets = cardArgs.Card.Targets.ToList();
            var chosenTargets = cardArgs.Targets.ToList();
            
            if (requiredTargets.Count != chosenTargets.Count)
            {
                throw new TargetCountMismatchException(requiredTargets.Count, chosenTargets.Count);
            }
            
            foreach (var (targetData, chosenTarget) in requiredTargets.Zip(chosenTargets))
            {
                if (!targetData.ValidateContext(new TargetValidationContext(gameState, chosenTarget)))
                {
                    throw new InvalidTargetChosenException();
                }
            }
        }
    }
}