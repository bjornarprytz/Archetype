using Archetype.Framework.Definitions;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Runtime.Implementation;

public class ActionQueue : IActionQueue
{
    private readonly IEventHistory _eventHistory;
    private readonly IDefinitions _definitions;
    
    private readonly Queue<IResolutionContext> _contextQueue = new();
    private readonly Queue<Effect> _effectQueue = new();

    public ActionQueue(IEventHistory eventHistory, IDefinitions definitions)
    {
        _eventHistory = eventHistory;
        _definitions = definitions;
    }

    public IResolutionContext? CurrentContext { get; private set; }

    public void Push(IResolutionContext context)
    {
        _contextQueue.Enqueue(context);
    }

    public IEvent? ResolveNext()
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
        CurrentContext!.Events.Add(e);
        
        if (_effectQueue.Count == 0)
        {
            _eventHistory.Push(new ActionBlockEvent(CurrentContext));
            
            CurrentContext = null;
        }
        
        return e;
    }

    private IEvent Resolve(Effect payload)
    {
        if (_definitions.Keywords[payload.Keyword] is not EffectDefinition effectDefinition)
            throw new InvalidOperationException($"Keyword ({payload.Keyword}) is not an effect");

        if (CurrentContext == null)
            throw new InvalidOperationException("No current context");
        
        return effectDefinition.Resolve(CurrentContext, _definitions, payload);
    }

    private bool TryAdvanceContext()
    {
        if (_contextQueue.Count == 0)
        {
            return false;
        }
        
        CurrentContext = _contextQueue.Dequeue();

        if (CurrentContext.Effects.Count == 0)
        {
            throw new InvalidOperationException("Context has no effects");
        }

        foreach (var effect in CurrentContext.Effects)
        {
            _effectQueue.Enqueue(effect);
        }

        return true;
    }
}