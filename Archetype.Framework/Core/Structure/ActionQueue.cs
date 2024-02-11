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
        if (_currentFrame?.Context is not { } context)
        {
            throw new InvalidOperationException("No frame/context to resolve prompt");
        }
        
        if (!context.PromptResponses.TryGetValue(promptContext.PromptId, out var prompt))
        {
            throw new InvalidOperationException("Prompt not found in context");
        }
        
        if (!prompt.IsPending)
        {
            throw new InvalidOperationException("Prompt has already been answered");
        }
        
        prompt.Answer(promptContext.Selection);
        
        return EffectResult.Resolved;
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
        if (!TryPopKeywordInstance(out var keywordInstance))
        {
            return null;
        }

        var result = Resolve(keywordInstance);
        
        if (_keywordStack.Count == 0)
        {
            var e = new ActionBlockEvent(_currentFrame.Context);
            eventBus.Publish(e);
            _currentFrame = null;
        }
        
        return result;
    }
    private IEffectResult? Resolve(IKeywordInstance keywordInstance)
    {
        var effectDefinition = rules.GetOrThrow<IEffectDefinition>(keywordInstance.Keyword);

        var result = effectDefinition.Resolve(_currentFrame!.Context, keywordInstance);
        
        var e = new EffectEvent(_currentFrame!.Context.Source, keywordInstance, result);

        if (result is IKeywordFrame keywordFrame)
        {
            _keywordFrames.Push((keywordFrame, e));
            
            if (keywordFrame.Effects.Count == 0)
            {
                throw new InvalidOperationException("Keyword frame has no effects");
            }

            foreach (var nestedKeyword in keywordFrame.Effects)
            {
                _keywordStack.Enqueue(nestedKeyword);
            }

            return Resolve(_keywordStack.Pop());
        }
        
        PushEvent(keywordInstance, e);

        return result;
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
        if (_keywordFrames.TryPop(out var item))
        {
            var (currentKeywordFrame, compositeEvent) = item;
            
            if (!currentKeywordFrame.Effects.Contains(keywordInstance))
            {
                throw new InvalidOperationException("Keyword instance not found in keyword frame");
            }
            
            e.Parent = compositeEvent;
            compositeEvent.Children.Add(e);
            
            if (currentKeywordFrame.Effects.Count > compositeEvent.Children.Count)
            {
                _keywordFrames.Push(item);
            } 
            else if (_keywordFrames.Count == 0)
            {
                _currentFrame!.Context.Events.Add(compositeEvent);
            }
        }
        else
        {
            _currentFrame!.Context.Events.Add(e);
        }
    }
}