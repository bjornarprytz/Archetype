using Archetype.Framework.GameLoop;

namespace Archetype.Framework.Events;

public interface IEventHistory
{
    IEnumerable<IEvent> GetEvents();
}

internal class EventHistory : IEventHistory
{
    public IEnumerable<IEvent> GetEvents()
    {
        throw new NotImplementedException();
    }
}