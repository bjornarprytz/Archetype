using Archetype.Framework.GameLoop;

namespace Archetype.Framework.Events;

public interface IEventHistory
{
    IEnumerable<Event> GetEvents();
}

public class EventHistory : IEventHistory
{
    public IEnumerable<Event> GetEvents()
    {
        throw new NotImplementedException();
    }
}