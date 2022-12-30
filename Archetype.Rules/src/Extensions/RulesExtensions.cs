using Archetype.Core.Effects;
using Archetype.Core.Extensions;
using Archetype.Core.Infrastructure;

namespace Archetype.Rules.Extensions;

internal static class RulesExtensions
{
     // TODO: These functions should probably return something
     
     public static IResult ResolveUpkeep(this IGameState gameState, Random random)
     {
          var cardToDraw = gameState.Player.DrawPile.Draw();
          
          return cardToDraw.MoveTo(gameState.Player.Hand);
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