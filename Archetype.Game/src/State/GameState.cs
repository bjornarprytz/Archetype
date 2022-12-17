using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Cards;
using Archetype.Core.Atoms.Zones;
using Archetype.Core.Infrastructure;
using Archetype.Game.Extensions;

namespace Archetype.Game.State;

internal record GameState(int Seed, IPlayer Player, IMap Map, ILocation? CurrentLocation, IResolution ResolutionZone) : IGameState
{
    public static IGameState Init(int seed)
    {
        var random = Static.SetRandomSeed(seed);

        var map = StateGeneration.GenerateMap(random, 5);
        var player = StateGeneration.GeneratePlayer(random);
        
        var gameState = new GameState(
            seed, 
            player, 
            map, 
            null, 
            new ResolutionZone());

        return gameState;
    }
}

internal class ResolutionZone : Zone<ICard>, IResolution
{
}
