using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Runtime.Implementation;

public class GameRoot : IGameRoot
{
    public GameRoot(IMetaGameState metaGameState, IGameState gameState, IInfrastructure infrastructure)
    {
        MetaGameState = metaGameState;
        GameState = gameState;
        Infrastructure = infrastructure;
    }

    public IMetaGameState MetaGameState { get; }
    public IGameState GameState { get; }
    public IInfrastructure Infrastructure { get; }
}