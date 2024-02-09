using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Design;
using Archetype.Framework.Extensions;

namespace Archetype.Framework.Core.Structure;

public interface IActionQueue
{
    IEffectResult? ResolvePrompt(IPromptContext promptContext);
    IEffectResult? Push(IResolutionFrame resolutionFrame, IPaymentContext paymentContext);
    IEffectResult? ResolveNextKeyword();
}

public class ActionQueue(IEventBus eventBus, IRules rules) : IActionQueue
{
    private IResolutionFrame? _currentFrame;
    // Card scope
    private readonly Queue<IResolutionFrame> _resolutionFrames = new();
    
    private readonly QueueStack<IKeywordInstance> _keywordStack = new();
    // Composite keyword scope
    private readonly Stack<(IKeywordFrame, IEffectEvent)> _keywordFrames = new();


    public IEffectResult? ResolvePrompt(IPromptContext promptContext)
    {
        throw new NotImplementedException();
    }

    public IEffectResult? Push(IResolutionFrame resolutionFrame, IPaymentContext paymentContext)
    {
        if (paymentContext.MakeDryRuns() is FailureResult failure)
        {
            return failure;
        }
        
        if (paymentContext.ResolvePayments() is { } paymentEvents)
        {
            var paymentEvent = new PaymentEvent(paymentContext.ResolutionContext.Source)
            {
                Children = new List<IEvent>(paymentEvents)
            };
            
            eventBus.Publish(paymentEvent);
        }
        
        _resolutionFrames.Enqueue(resolutionFrame);
        
        return EffectResult.Resolved;
    }

    public IEffectResult? ResolveNextKeyword()
    {
        if (!TryPopKeywordInstance(out var payload))
        {
            return null;
        }

        if (Resolve(payload) is { } promptDescription)
        {
            return promptDescription;
        }
        

        if (_keywordStack.Count == 0)
        {
            eventBus.Publish(new ActionBlockEvent(_currentFrame.Context));
            _currentFrame = null;
        }
        
        return null;
    }
    private IPromptDescription? Resolve(IKeywordInstance keywordInstance)
    {
        var effectDefinition = rules.GetOrThrow<IEffectDefinition>(keywordInstance.Keyword);

        var result = effectDefinition.Resolve(_currentFrame!.Context, keywordInstance);
        
        var e = new EffectEvent(_currentFrame!.Context.Source, keywordInstance, result);

        if (result is IKeywordFrame keywordFrame)
        {
            _keywordFrames.Push((keywordFrame, e));

            return null;
        }
        
        PushEvent(keywordInstance, e);
        
        if(result is IPromptDescription promptDescription)
        {
            return promptDescription;
        }
        
        return null;
    }

    private bool TryAdvanceResolutionFrame()
    {
        if (!_resolutionFrames.TryDequeue(out var nextFrame))
        {
            return false;
        }
        
        _currentFrame = nextFrame;
        
        if (_currentFrame.Effects.Count == 0)
        {
            throw new InvalidOperationException("Next resolution frame has no effects");
        }
        
        foreach (var effect in _currentFrame.Effects)
        {
            _keywordStack.Enqueue(effect);
        }
        
        return true;
    }

    private bool TryPopKeywordInstance(out IKeywordInstance keywordInstance)
    {
        keywordInstance = default!;
        
        if (_currentFrame == null && !TryAdvanceResolutionFrame())
        {
            return false;
        }

        return _keywordStack.TryPop(out keywordInstance);
    }

    private void PushEvent(IKeywordInstance keywordInstance, IEvent e)
    {
        // Figure out which keyword frame the event belongs to
        // Keyword frames that are "done" are naturally popped off the stack here,
        // but the current one is pushed back onto the stack
        while (_keywordFrames.TryPop(out var item))
        {
            var (currentKeywordFrame, compositeEvent) = item;
            
            if (!currentKeywordFrame.Effects.Contains(keywordInstance)) continue;
            
            e.Parent = compositeEvent;
            compositeEvent.Children.Add(e);
            _keywordFrames.Push(item);
            break;
        }
        
        // If there are no keyword frames, we've fully resolved a top level keyword,
        // and that means that e is the root event of that keyword.
        if (_keywordFrames.Count == 0)
        {
            _currentFrame!.Context.Events.Add(e);
        }
    }
}