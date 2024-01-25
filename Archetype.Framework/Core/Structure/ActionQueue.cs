using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Design;
using Archetype.Framework.Extensions;

namespace Archetype.Framework.Core.Structure;

public interface IActionQueue
{
    IResolutionFrame? CurrentFrame { get; }
    void Push(IResolutionFrame frame);
    IPromptDescription? ResolveNextKeyword();
}

public class ActionQueue(IEventBus eventBus, IRules rules) : IActionQueue
{
    public IResolutionFrame? CurrentFrame { get; private set; }

    private IEvent? _currentFrameEventTree;
    // Card scope
    private readonly Queue<IResolutionFrame> _resolutionFrames = new();
    
    private readonly QueueStack<IKeywordInstance> _keywordStack = new();
    // Composite keyword scope
    private readonly Stack<(IKeywordFrame, IEffectEvent)> _keywordFrames = new();


    public void Push(IResolutionFrame context)
    {
        _resolutionFrames.Enqueue(context);
    }

    public IPromptDescription? ResolveNextKeyword()
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
            eventBus.Publish(new ActionBlockEvent(CurrentFrame.Context));
            CurrentFrame = null;
        }
        
        return null;
    }
    private IPromptDescription? Resolve(IKeywordInstance keywordInstance)
    {
        var definition = rules.GetDefinition(keywordInstance.Keyword);
        
        if (definition == null)
        {
            throw new InvalidOperationException($"No definition found for effect {keywordInstance.Keyword}");
        }

        var result = definition.Resolve(CurrentFrame!.Context, keywordInstance);
        
        var e = new EffectEvent(CurrentFrame!.Context.Source, keywordInstance, result);

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

    private bool TryPopKeywordInstance(out IKeywordInstance keywordInstance)
    {
        keywordInstance = default!;
        
        if (CurrentFrame == null && !TryAdvanceResolutionFrame())
        {
            return false;
        }

        return _keywordStack.TryPop(out keywordInstance);
    }

    private void PushEvent(IKeywordInstance keywordInstance, IEvent e)
    {
        while (_keywordFrames.TryPop(out var item))
        {
            var (currentKeywordFrame, compositeEvent) = item;
            
            if (!currentKeywordFrame.Effects.Contains(keywordInstance)) continue;
            
            e.Parent = compositeEvent;
            compositeEvent.Children.Add(e);
            _keywordFrames.Push(item);
            break;
        }
        
        if (_keywordFrames.Count == 0)
        {
            CurrentFrame!.Context.Events.Add(e);
        }
    }
}