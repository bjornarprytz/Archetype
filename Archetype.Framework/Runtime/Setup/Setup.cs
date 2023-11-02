using Archetype.Framework.Proto;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Runtime.Setup;

/**
 * TODO: Work on this sketch
public interface IArchetypeGame
{
    IGameActionHandler GameActionHandler { get; }
    IGameState GameState { get; }
    IEventBus EventBus { get; }
}

public class ArchetypeBuilder
{
    private GameRoot _gameRoot = new GameRoot();

    public ArchetypeBuilder RegisterRules(IRules rules)
    {
        _gameRoot.MetaGameState.Rules = rules;
        return this;
    }

    private class MetaGameState : IMetaGameState
    {
        public IRules Rules { get; private set; } = null!;
        public IProtoCards ProtoCards { get; private set; } = null!;
    }
    

    private class GameRoot : IGameRoot
    {
        public IMetaGameState MetaGameState { get; private set; } = new MetaGameState()!;
        public IGameState GameState { get; private set; } = null!;
        public IInfrastructure Infrastructure { get; private set; } = null!;
    }
}
 */
