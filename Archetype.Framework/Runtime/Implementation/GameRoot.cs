using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Runtime.Implementation;

public class GameRoot(IMetaGameState metaGameState, IGameState gameState, IInfrastructure infrastructure)
    : IGameRoot
{
    public IMetaGameState MetaGameState { get; } = metaGameState;
    public IGameState GameState { get; } = gameState;
    public IInfrastructure Infrastructure { get; } = infrastructure;
}