using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Runtime.Implementation;

public class ActionQueue : IActionQueue
{
    private readonly IEventHistory _eventHistory;
    private readonly IDefinitions _definitions;
    
    private readonly Queue<IResolutionFrame> _frameQueue = new();
    private readonly Queue<EffectInstance> _effectQueue = new();

    public ActionQueue(IEventHistory eventHistory, IDefinitions definitions)
    {
        _eventHistory = eventHistory;
        _definitions = definitions;
    }

    public IResolutionFrame? CurrentFrame { get; private set; }

    public void Push(IResolutionFrame context)
    {
        _frameQueue.Enqueue(context);
    }

    public IEvent? ResolveNext()
    {
        if (_effectQueue.Count == 0)
        {
            if (!TryAdvanceFrame())
            {
                return null;
            }
        }
        
        var payload = _effectQueue.Dequeue();
        var e = Resolve(payload.CreateEffect(CurrentFrame!.Context));
        CurrentFrame!.Context.Events.Add(e);
        
        if (_effectQueue.Count == 0)
        {
            _eventHistory.Push(new ActionBlockEvent(CurrentFrame.Context));
            
            CurrentFrame = null;
        }
        
        return e;
    }

    private IEvent Resolve(Effect payload)
    {
        if (_definitions.Keywords[payload.Keyword] is not EffectDefinition effectDefinition)
            throw new InvalidOperationException($"Keyword ({payload.Keyword}) is not an effect");

        if (CurrentFrame == null)
            throw new InvalidOperationException("No current context");
        
        return effectDefinition.Resolve(CurrentFrame.Context, _definitions, payload);
    }

    private bool TryAdvanceFrame()
    {
        if (_frameQueue.Count == 0)
        {
            return false;
        }
        
        CurrentFrame = _frameQueue.Dequeue();

        if (CurrentFrame.Effects.Count == 0)
        {
            throw new InvalidOperationException("Context has no effects");
        }

        foreach (var effect in CurrentFrame.Effects)
        {
            _effectQueue.Enqueue(effect);
        }

        return true;
    }
}