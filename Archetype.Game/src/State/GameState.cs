using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Cards;
using Archetype.Core.Atoms.Zones;
using Archetype.Core.Infrastructure;
using Archetype.Game.Extensions;

namespace Archetype.Game.State;

internal record GameState(IPlayer Player, IMap Map, ILocation? CurrentLocation, IResolution ResolutionZone) : IGameState
{
    public static IGameState Init(Random random)
    {
        var map = StateGeneration.GenerateMap(random, 5);
        var player = StateGeneration.GeneratePlayer(random);
        
        var gameState = new GameState(
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
