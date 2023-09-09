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
    public void Push(Effect payload);
    public IEnumerable<Effect> Effects { get; }
    public Event? ResolveNext();
    public IEnumerable<Event> ResolveAll();
}

public interface IGameActionHandler
{
    public ActionResult Handle(IRequest args);
}

public class ActionDescription
{
    string Name { get; set; }
}

public class ActionResult
{
    public IReadOnlyList<Event> Events { get; set; }
    public IReadOnlyList<ActionDescription> AvailableActions { get; set; }
}