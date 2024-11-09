using Archetype.Framework.GameLoop;

namespace Archetype.Framework.Events;

public interface IEventHistory
{
    public Scope GetRootScope();
    IEnumerable<Event> GetEvents();
}

public class EventHistory : IEventHistory
{
    public Scope GetRootScope()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Event> GetEvents()
    {
        throw new NotImplementedException();
    }
}