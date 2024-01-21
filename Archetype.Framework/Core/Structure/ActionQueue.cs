using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Design;
using Archetype.Framework.Extensions;

namespace Archetype.Framework.Core.Structure;

public interface IActionQueue
{
    IResolutionFrame? CurrentFrame { get; }
    void Push(IResolutionFrame frame);
    IEvent? ResolveNextKeyword();
}

public class ActionQueue(IEventBus eventBus, IRules rules) : IActionQueue
{
    public IResolutionFrame? CurrentFrame { get; private set; }
    
    // Card scope
    private readonly Queue<IResolutionFrame> _resolutionFrames = new();
    
    private readonly QueueStack<IKeywordInstance> _keywordStack = new();
    // Composite keyword scope
    private readonly Stack<IKeywordFrame> _keywordFrames = new();


    public void Push(IResolutionFrame context)
    {
        _resolutionFrames.Enqueue(context);
    }

    public IEvent? ResolveNextKeyword()
    {
        if (!TryPopPrimitive(out var payload))
        {
            return null;
        }

        var e = Resolve(payload);
        PushEvent(payload, e);

        if (_keywordStack.Count == 0)
        {
            eventBus.Publish(new ActionBlockEvent(CurrentFrame.Context));
            CurrentFrame = null;
        }
        
        return e;
    }
    private IEvent Resolve(EffectPayload payload)
    {
        var definition = rules.GetDefinition(payload.Keyword);

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

    private bool TryPopPrimitive(out EffectPayload payload)
    {
        if (CurrentFrame == null && !TryAdvanceResolutionFrame())
        {
            payload = default!;
            return false;
        }

        while (_keywordStack.TryPop(out var keywordInstance))
        {
            payload = keywordInstance.BindPayload(CurrentFrame!.Context);
            
            if (rules.GetDefinition(keywordInstance.ResolveFuncName) is not IEffectCompositeDefinition compositeDefinition)
            {
                return true;
            }
            
            var keywordFrame = compositeDefinition.Compose(CurrentFrame.Context, payload);

            PushEvent(payload, keywordFrame.Event);
            _keywordFrames.Push(keywordFrame);
            
            foreach (var instance in keywordFrame.Effects.Reverse())
            {
                _keywordStack.Push(instance);
            }
        }
        
        payload = default!;
        return false;
    }

    private void PushEvent(EffectPayload payload, IEvent e)
    {
        while (_keywordFrames.TryPop(out var currentKeywordFrame))
        {
            if (currentKeywordFrame.Effects.All(ki => ki.Id != payload.Id)) continue;
            
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