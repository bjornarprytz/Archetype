using Archetype.Rules;
using Archetype.Rules.Definitions;
using Archetype.Rules.State;

namespace Archetype.Runtime.Implementation;

public class EffectQueue : IEffectQueue
{
    private readonly IEventHistory _eventHistory;
    private readonly GameState _state;
    private readonly Definitions _definitions;
    
    private readonly Queue<Effect> _queue = new();

    public EffectQueue(IEventHistory eventHistory, GameState state, Definitions definitions)
    {
        _eventHistory = eventHistory;
        _state = state;
        _definitions = definitions;
    }

    public void Push(Effect payload)
    {
        _queue.Enqueue(payload);
    }

    public IEnumerable<Effect> Effects => _queue;
    public Event? ResolveNext()
    {
        if (_queue.Count == 0)
            return null;
        
        var payload = _queue.Dequeue();
        var e = Resolve(payload);
        _eventHistory.Push(e);
        
        return e;
    }

    public IEnumerable<Event> ResolveAll()
    {
        var events = new List<Event>();
        
        while (ResolveNext() is {} e)
        {
            events.Add(e);
        }

        return events;
    }

    private Event Resolve(Effect payload)
    {
        if (_definitions.Keywords[payload.Keyword] is not EffectDefinition effectDefinition)
            throw new InvalidOperationException($"Keyword ({payload.Keyword}) is not an effect");

        return effectDefinition.Resolve(_state, _definitions, payload);
    }
}