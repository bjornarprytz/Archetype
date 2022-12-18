using Archetype.Core.Infrastructure;

namespace Archetype.Game;

public class Archetype
{
    public static IArchetypeGame NewGame(int seed) =>  ArchetypeGame.Create(seed);
    public static IArchetypeGame LoadGame(IGameState gameState, int seed) =>  ArchetypeGame.Load(gameState, seed);
}