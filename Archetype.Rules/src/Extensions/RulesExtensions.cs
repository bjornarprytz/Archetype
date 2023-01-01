using Archetype.Core.Effects;
using Archetype.Core.Infrastructure;

namespace Archetype.Rules.Extensions;

internal static class RulesExtensions
{
     public static IResult ResolveUpkeep(this IGameState gameState, Random random)
     {
          return gameState.Player.DrawCard();
     }

     public static IResult ResolveCombat(this IGameState gameState, Random random)
     {
          // TODO: Resolve combat in all nodes
          
          return IResult.Empty();
     }

     public static IResult ResolveMovement(this IGameState gameState, Random random)
     {
          // TODO: Resolve movement for all enemy units
          
          return IResult.Empty();
     }

     public static IResult CheckForGameOver(this IGameState gameState, Random random)
     {
          // TODO: Check for player death and other game over conditions
          return IResult.Empty();
     }
}