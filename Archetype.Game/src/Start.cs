namespace Archetype.Game;

public class Archetype
{
    public static IArchetypeGame NewGame(int seed) =>  ArchetypeGame.Create(seed);
    public static Task<IArchetypeGame> LoadGameAsync(Guid gameId) =>  ArchetypeGame.Load(gameId);
}