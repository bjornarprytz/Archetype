using Archetype.Core.Infrastructure;

namespace Archetype.Rules.Extensions;

internal static class RulesExtensions
{
     // TODO: These functions should probably return something
     
     public static void ResolveUpkeep(this IGameState gameState, Random random)
     {
          // TODO: Check hand size and draw cards etc.
     }
     
     public static void ResolveCombat(this IGameState gameState, Random random)
     {
          // TODO: Resolve combat in all nodes
     }

     public static void ResolveMovement(this IGameState gameState, Random random)
     {
          // TODO: Resolve movement for all enemy units
     }

     public static void CheckForGameOver(this IGameState gameState, Random random)
     {
          // TODO: Check for player death and other game over conditions
     }
}