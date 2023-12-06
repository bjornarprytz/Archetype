using Archetype.Framework.Core.Primitives;
using Archetype.Framework.State;
using PubSub;

namespace Archetype.Framework.Core.Structure;

public interface IEventHistory
{
    IReadOnlyList<IEvent> KeywordEvents { get; }
    IReadOnlyList<IActionBlockEvent> ActionBlockEvents { get; }
}

public interface IEventBus
{
    void Publish<T>(T @event) where T : IActionBlockEvent;
    void Subscribe<T>(IAtom subscriber, Action<T> handler) where T : IEvent;
    void Unsubscribe<T>(IAtom subscriber, Action<T> handler) where T : IEvent;
    
}

public class EventBus : IEventBus, IEventHistory
{
    private readonly Hub _hub= new();
    private readonly List<IActionBlockEvent> _actionBlockEvents = new();
    private readonly List<IEvent> _keywordEvents = new();
    
    public void Publish<T>(T @event) where T : IActionBlockEvent
    {
        _actionBlockEvents.Add(@event);

        AddSubEvents(@event);

        return;

        void AddSubEvents(IEvent e)
        {
            foreach (var child in e.Children.Where(s => s is not NonEvent))
            {
                AddSubEvents(child);
                _keywordEvents.Add(child);
                _hub.Publish(child);
            }
        }
    }
    
    public void Subscribe<T>(IAtom subscriber, Action<T> handler) where T : IEvent => _hub.Subscribe(subscriber, handler);
    public void Unsubscribe<T>(IAtom subscriber, Action<T> handler) where T : IEvent => _hub.Unsubscribe(subscriber, handler);

    public IReadOnlyList<IEvent> KeywordEvents => _keywordEvents;
    public IReadOnlyList<IActionBlockEvent> ActionBlockEvents => _actionBlockEvents;
}