using System;
using System.Linq;
using Archetype.Game.Exceptions;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Infrastructure;

namespace Archetype.Game.Extensions
{
    internal static class CardExtensions
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
                if (chosenTarget is IUnit { CurrentZone: IMapNode node }
                    && node.DistanceTo(cardArgs.Whence) is var distance and > -1 
                    && distance > cardArgs.Card.Range)
                {
                    throw new InvalidOperationException(
                        $"Target is too far from whence. Card Range: {cardArgs.Card.Range}, Distance: {distance}");
                }
                
                if (!targetData.ValidateContext(new TargetValidationContext(gameState, chosenTarget)))
                {
                    throw new InvalidTargetChosenException();
                }
            }
        }
    }
}