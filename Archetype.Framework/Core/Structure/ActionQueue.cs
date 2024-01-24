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

    private IEvent? CurrentFrameEventTree;
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
        if (!TryPopPayload(out var payload))
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
        var definition = rules.GetDefinition(payload.EffectId);
        
        if (definition == null)
        {
            throw new InvalidOperationException($"No definition found for effect {payload.EffectId}");
        }

        var result = definition.Resolve(CurrentFrame!.Context, payload);

        if (result is IKeywordFrame keywordFrame)
        {
            _keywordFrames.Push(keywordFrame);
        }
        
        return new EffectEvent(payload, result);
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

    private bool TryPopPayload(out EffectPayload payload)
    {
        if (CurrentFrame == null && !TryAdvanceResolutionFrame())
        {
            payload = default!;
            return false;
        }

        if (_keywordStack.TryPop(out var keywordInstance))
        {
            payload = keywordInstance.BindPayload(CurrentFrame!.Context);

            return true;
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