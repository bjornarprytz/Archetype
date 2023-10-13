using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;

namespace Archetype.Framework.Runtime.Implementation;

public class ActionQueue : IActionQueue
{
    private readonly IEventBus _eventBus;
    private readonly IRules _rules;

    public IResolutionFrame? CurrentFrame { get; private set; }
    
    // Card scope
    private readonly Queue<IResolutionFrame> _resolutionFrames = new();
    
    private readonly QueueStack<IKeywordInstance> _keywordStack = new();
    // Composite keyword scope
    private readonly Stack<IKeywordFrame> _keywordFrames = new();
    
    public ActionQueue(IEventBus eventBus, IRules rules)
    {
        _eventBus = eventBus;
        _rules = rules;
    }


    public void Push(IResolutionFrame context)
    {
        _resolutionFrames.Enqueue(context);
    }

    public IEvent? ResolveNextKeyword()
    {
        if (TryPopPrimitive(out var keywordInstance))
        {
            return null;
        }
        
        var payload = keywordInstance.BindPayload(CurrentFrame!.Context);

        var e = Resolve(payload);
        PushEvent(keywordInstance, e);

        if (_keywordStack.Count == 0)
        {
            _eventBus.Publish(new ActionBlockEvent(CurrentFrame.Context));
            CurrentFrame = null;
        }
        
        return e;
    }
    private IEvent Resolve(EffectPayload payload)
    {
        var definition = _rules.GetDefinition(payload.Keyword);

        if (definition is IEffectPrimitiveDefinition primitive)
        {
            return primitive.Resolve(CurrentFrame!.Context, payload);
        }
        
        throw new InvalidOperationException($"Keyword ({payload.Keyword}) is not an effect");
    }

    private bool TryAdvanceResolutionFrame()
    {
        if (!_resolutionFrames.TryDequeue(out var nextFrame))
        {
            return false;
        }
        
        CurrentFrame = nextFrame;
        
        if (CurrentFrame.Effects.Count == 0)
        {
            throw new InvalidOperationException("Next resolution frame has no effects");
        }
        
        foreach (var cost in CurrentFrame.Costs)
        {
            _keywordStack.Enqueue(cost);
        }
        
        foreach (var effect in CurrentFrame.Effects)
        {
            _keywordStack.Enqueue(effect);
        }
        
        return true;
    }

    private bool TryPopPrimitive(out IKeywordInstance keywordInstance)
    {
        if (CurrentFrame == null && !TryAdvanceResolutionFrame())
        {
            keywordInstance = default!;
            return false;
        }

        while (_keywordStack.TryPop(out keywordInstance))
        {
            if (_rules.GetDefinition(keywordInstance.Keyword) is not IEffectCompositeDefinition compositeDefinition)
            {
                return true;
            }
            
            var payload = keywordInstance.BindPayload(CurrentFrame!.Context);
            var keywordFrame = compositeDefinition.Compose(CurrentFrame.Context, payload);

            PushEvent(keywordInstance, keywordFrame.Event);
            _keywordFrames.Push(keywordFrame);
            
            foreach (var instance in keywordFrame.Effects.Reverse())
            {
                _keywordStack.Push(instance);
            }
        }
        
        keywordInstance = default!;
        return false;
    }

    private void PushEvent(IKeywordInstance keywordInstance, IEvent e)
    {
        while (_keywordFrames.TryPop(out var currentKeywordFrame))
        {
            if (!currentKeywordFrame.Effects.Contains(keywordInstance)) continue;
            
            e.Parent = currentKeywordFrame.Event;
            currentKeywordFrame.Event.Children.Add(e);
            _keywordFrames.Push(currentKeywordFrame);
            break;
        }
        
        if (_keywordFrames.Count == 0)
        {
            CurrentFrame!.Context.Events.Add(e);
        }
    }
}