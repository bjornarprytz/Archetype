namespace Archetype.Framework.Core.Structure;

public interface IInfrastructure
{
    IEventHistory EventHistory { get; }
    IActionQueue ActionQueue { get; }
    IGameLoop GameLoop { get; }
    IGameActionHandler GameActionHandler { get; }
}

public class Infrastructure(IEventHistory eventHistory, IActionQueue actionQueue, IGameLoop gameLoop,
        IGameActionHandler gameActionHandler)
    : IInfrastructure
{
    public IEventHistory EventHistory { get; } = eventHistory;
    public IActionQueue ActionQueue { get; } = actionQueue;
    public IGameLoop GameLoop { get; } = gameLoop;
    public IGameActionHandler GameActionHandler { get; } = gameActionHandler;
}