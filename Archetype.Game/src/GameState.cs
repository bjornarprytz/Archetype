using Archetype.Core;
using Archetype.Core.Atoms;
using Archetype.Core.Infrastructure;

namespace Archetype.Game;

internal record GameState(int Seed, IPlayer Player, ICard? CurrentLocation, IWorld WorldMap) : IGameState
{
    public static IGameState Init(int seed)
    {
        var gameState = new GameState(seed, default!, null, default!);
        
        // TODO: Actually generate some game state

        return gameState;
    }
}
