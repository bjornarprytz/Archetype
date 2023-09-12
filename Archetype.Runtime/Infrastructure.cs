using Archetype.Rules;
using MediatR;

namespace Archetype.Runtime;

public interface IEventHistory
{
    public void Push(Event e);
    public IReadOnlyList<Event> Events { get; set; }
}

public interface IEffectQueue
{
    void Push(ResolutionContext context);
    Event? ResolveNext();
}

public interface IGameLoop
{
    ActionResult Advance();
}

public interface IGameActionHandler
{
    public ActionResult Handle(IRequest args);
}

public class ActionDescription
{
    public string Name { get; set; }
    
    // TODO: Describe requirements of the action
}

public class ActionResult
{
    public IReadOnlyList<ActionDescription> AvailableActions { get; set; }
}