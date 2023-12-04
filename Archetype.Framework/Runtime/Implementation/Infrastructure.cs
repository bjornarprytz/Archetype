using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Runtime.Implementation;

public class Infrastructure : IInfrastructure
{
    public Infrastructure(IEventHistory eventHistory, IActionQueue actionQueue, IGameLoop gameLoop, IGameActionHandler gameActionHandler)
    {
        EventHistory = eventHistory;
        ActionQueue = actionQueue;
        GameLoop = gameLoop;
        GameActionHandler = gameActionHandler;
    }
    
    public IEventHistory EventHistory { get; }
    public IActionQueue ActionQueue { get; }
    public IGameLoop GameLoop { get; }
    public IGameActionHandler GameActionHandler { get; }
}