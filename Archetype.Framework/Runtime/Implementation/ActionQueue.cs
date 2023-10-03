using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;

namespace Archetype.Framework.Runtime.Implementation;

public class ActionQueue : IActionQueue
{
    private readonly IEventHistory _eventHistory;
    private readonly IDefinitions _definitions;
    
    private readonly Queue<IResolutionFrame> _frameQueue = new();
    private readonly Queue<IKeywordInstance> _effectQueue = new();

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
        
        var effectInstance = _effectQueue.Dequeue();
        
        var e = Resolve(effectInstance);
        CurrentFrame!.Context.Events.Add(e);
        
        if (_effectQueue.Count == 0)
        {
            _eventHistory.Push(new ActionBlockEvent(CurrentFrame.Context));
            
            CurrentFrame = null;
        }
        
        return e;
    }

    private IEvent Resolve(IKeywordInstance effectInstance)
    {
        if (_definitions.GetDefinition(effectInstance.Keyword) is not EffectPrimitiveDefinition effectDefinition)
            throw new InvalidOperationException($"Keyword ({effectInstance.Keyword}) is not an effect primitive");

        if (CurrentFrame == null)
            throw new InvalidOperationException("No current context");
        
        var payload = effectInstance.BindPayload(CurrentFrame!.Context);
        
        return effectDefinition.Resolve(CurrentFrame.Context, payload);
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
            throw new InvalidOperationException("Next resolution frame has no effects");
        }

        foreach (var cost in CurrentFrame.Costs.Select(c => c.ComposePrimitives(_definitions, CurrentFrame.Context)))
        {
            _effectQueue.Enqueue(cost);
        }
        
        foreach (var effect in CurrentFrame.Effects.Select(e => e.ComposePrimitives(_definitions, CurrentFrame.Context)))
        {
            _effectQueue.Enqueue(effect);
        }

        return true;
    }
}