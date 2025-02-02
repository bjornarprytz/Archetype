using Archetype.Framework.GameLoop;

namespace Archetype.Framework.Events;

public interface IGameEvents
{
    IEnumerable<IEvent> GetEvents();
}