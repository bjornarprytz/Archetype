using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Zones;
using Archetype.Core.Infrastructure;

namespace Archetype.Game.State;

internal record GameState(int Seed, IPlayer Player, ILocation? CurrentLocation, IResolution ResolutionZone) : IGameState
{
    public static IGameState Init(int seed)
    {
        var gameState = new GameState(seed, default!, null, default!);
        
        // TODO: Actually generate some game state

        return gameState;
    }
}
