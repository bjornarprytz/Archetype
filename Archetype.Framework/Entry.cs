using Archetype.Framework.Events;
using Archetype.Framework.GameLoop;
using Archetype.Framework.State;

namespace Archetype.Framework;

public static class Bootstrap
{
    public static IGameRoot StartGame(IGameState initialState, IRules rules)
    {
        return new GameRoot(initialState, rules);
    }
}

file class GameRoot(IGameState initialState, IRules rules) : IGameRoot
{
    public IRules Rules { get; } = rules;
    public IGameState State { get; } = initialState;
    public IGameLoop Loop { get; } = new Loop();
    public IGameEvents Events { get; } = new GameEvents();
}

file class GameEvents : IGameEvents
{
    private readonly List<IEvent> _events = new();
    
    public IEnumerable<IEvent> GetEvents()
    {
        return _events;
    }
}

file class Loop : IGameLoop
{
    public IScope GetCurrentScope()
    {
        throw new NotImplementedException();
    }
}