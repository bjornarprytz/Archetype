using Archetype.Rules;
using Archetype.Rules.Definitions;
using Archetype.Rules.State;

namespace Archetype.Runtime.Implementation;

public class EffectQueue : IEffectQueue
{
    private readonly IEventHistory _eventHistory;
    private readonly GameState _state;
    private readonly Definitions _definitions;
    
    private ResolutionContext? _currentContext;
    private readonly Queue<ResolutionContext> _contextQueue = new();
    private readonly Queue<Effect> _effectQueue = new();

    public EffectQueue(IEventHistory eventHistory, GameState state, Definitions definitions)
    {
        _eventHistory = eventHistory;
        _state = state;
        _definitions = definitions;
    }
    
    public void Push(ResolutionContext context)
    {
        _contextQueue.Enqueue(context);
    }

    public Event? ResolveNext()
    {
        if (_effectQueue.Count == 0)
        {
            if (!TryAdvanceContext())
            {
                return null;
            }
        }
        
        var payload = _effectQueue.Dequeue();
        var e = Resolve(payload);
        _eventHistory.Push(e);
        
        return e;
    }

    private Event Resolve(Effect payload)
    {
        if (_definitions.Keywords[payload.Keyword] is not EffectDefinition effectDefinition)
            throw new InvalidOperationException($"Keyword ({payload.Keyword}) is not an effect");

        if (_currentContext == null)
            throw new InvalidOperationException("No current context");
        
        return effectDefinition.Resolve(_state, _definitions, payload, _currentContext);
    }

    private bool TryAdvanceContext()
    {
        _currentContext = null;
            
        if (_contextQueue.Count == 0)
        {
            return false;
        }
        
        _currentContext = _contextQueue.Dequeue();

        if (_currentContext.Effects.Count == 0)
        {
            throw new InvalidOperationException("Context has no effects");
        }

        foreach (var effect in _currentContext.Effects)
        {
            _effectQueue.Enqueue(effect);
        }

        return true;
    }
}