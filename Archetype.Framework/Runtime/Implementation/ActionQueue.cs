using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;

namespace Archetype.Framework.Runtime.Implementation;

public class ActionQueue : IActionQueue
{
    private readonly IEventHistory _eventHistory;
    private readonly IDefinitions _definitions;
    
    // Card scope
    private readonly Queue<IResolutionFrame> _frameQueue = new();
    // Keyword scope
    private readonly QueueStack<IKeywordInstance> _keywordStack = new();

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
        if (_keywordStack.Count == 0)
        {
            if (!TryAdvanceFrame())
            {
                return null;
            }
        }

        if (GetNextPayload() is not { } payload)
        {
            return null;
        }

        var e = Resolve(payload);
        CurrentFrame!.Context.Events.Add(e);
        
        if (_keywordStack.Count == 0)
        {
            _eventHistory.Push(new ActionBlockEvent(CurrentFrame.Context));
            
            CurrentFrame = null;
        }
        
        return e;
    }

    private EffectPayload? GetNextPayload()
    {
        if (_keywordStack.Count == 0)
        {
            return null;
        }
        
        while (true)
        {
            var effectInstance = _keywordStack.Pop();
            var payload = effectInstance.BindPayload(CurrentFrame!.Context);

            if (_definitions.GetDefinition(effectInstance.Keyword) is EffectCompositeDefinition definition)
            {
                var keywordInstances = definition.Compose(CurrentFrame!.Context, payload);

                if (keywordInstances.Count == 0)
                {
                    throw new InvalidOperationException("Composite effect returned no effects");
                }

                // Push in reverse order so that the first effect is on top of the stack
                foreach (var keywordInstance in keywordInstances.Reverse())
                {
                    _keywordStack.Push(keywordInstance);
                }
            }
            else
            {
                return payload;
            }
        }
    }

    private IEvent Resolve(EffectPayload payload)
    {
        var definition = _definitions.GetDefinition(payload.Keyword);

        if (definition is EffectPrimitiveDefinition primitive)
        {
            return primitive.Resolve(CurrentFrame!.Context, payload);
        }
        
        throw new InvalidOperationException($"Keyword ({payload.Keyword}) is not an effect");
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
}