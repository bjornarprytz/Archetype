namespace Archetype.Framework.Core.Structure;

public interface IGameRoot
{
    IMetaGameState MetaGameState { get; }
    IGameState GameState { get; }
    IInfrastructure Infrastructure { get; }
}

public class GameRoot(IMetaGameState metaGameState, IGameState gameState, IInfrastructure infrastructure)
    : IGameRoot
{
    public IMetaGameState MetaGameState { get; } = metaGameState;
    public IGameState GameState { get; } = gameState;
    public IInfrastructure Infrastructure { get; } = infrastructure;
}