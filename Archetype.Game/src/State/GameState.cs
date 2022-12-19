using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Cards;
using Archetype.Core.Atoms.Zones;
using Archetype.Core.Infrastructure;
using Archetype.Core.Prompts;
using Archetype.Game.Extensions;

namespace Archetype.Game.State;

internal record GameState(IPlayer Player, IMap Map, ILocation? CurrentLocation, IResolution ResolutionZone, IPrompter Prompter) : IGameState
{
    public static IGameState Init(Random random)
    {
        var map = StateGeneration.GenerateMap(random, 5);
        var player = StateGeneration.GeneratePlayer(random);
        
        var gameState = new GameState(
            player, 
            map, 
            null,
            new ResolutionZone(),
            null); // TODO: Figure out a smart way to inject this. Maybe it's not part of the game state? That could in turn complicate save game state

        return gameState;
    }
}

internal class ResolutionZone : Zone<ICard>, IResolution
{
}
