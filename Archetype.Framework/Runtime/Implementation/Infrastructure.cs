using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Runtime.Implementation;

public class Infrastructure(IEventHistory eventHistory, IActionQueue actionQueue, IGameLoop gameLoop,
        IGameActionHandler gameActionHandler)
    : IInfrastructure
{
    public IEventHistory EventHistory { get; } = eventHistory;
    public IActionQueue ActionQueue { get; } = actionQueue;
    public IGameLoop GameLoop { get; } = gameLoop;
    public IGameActionHandler GameActionHandler { get; } = gameActionHandler;
}