using Archetype.BasicRules.Primitives;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime.Actions;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Runtime.Implementation;

public class GameLoop(IActionQueue actionQueue, IGameState gameState, IMetaGameState metaGameState)
    : IGameLoop
{
    private readonly IReadOnlyList<IPhase> _phases = metaGameState.Rules.TurnSequence;

    private readonly Queue<IPhase> _remainingPhases = new();
    private readonly Queue<IStep> _remainingSteps = new();


    public IPhase? CurrentPhase { get; private set; } = default;

    public IGameAPI Advance()
    {
        // TODO: Check victory conditions now and then (when?)
        
        CurrentPhase ??= MountNextPhase();
        
        while (true)
        {
            
            // Flush the action queue
            if (!ResolveActionQueue())
            {
                return new PromptApi();
            }
            
            // If we're in a phase, flush the steps
            if (_remainingSteps.Count > 0)
            {
                if (!ResolveSteps())
                {
                    return new PromptApi();
                }
            }
            
            if (CurrentPhase.AllowedActions.Count != 0)
            {
                return new GameAPI(CurrentPhase.AllowedActions);
            }

            CurrentPhase = MountNextPhase();
        }
    }

    public IGameAPI EndPhase()
    {
        CurrentPhase = MountNextPhase();

        return Advance();
    }

    private IPhase MountNextPhase()
    {
        if (_remainingPhases.Count == 0)
        {
            // Next turn
            foreach (var phase in _phases)
            {
                _remainingPhases.Enqueue(phase);
            }
        }
        
        var nextPhase = _remainingPhases.Dequeue();
        
        if (_remainingSteps.Count > 0)
        {
            throw new InvalidOperationException("Steps remaining");
        }

        foreach (var step in nextPhase.Steps)
        {
            _remainingSteps.Enqueue(step);
        }
        
        return nextPhase;
    }

    private bool ResolveSteps()
    {
        while (_remainingSteps.TryDequeue(out var step))
        {
            actionQueue.Push(CreateResolutionFrame(step));
                
            if (!ResolveActionQueue())
            {
                return false;
            }
        }

        return true;
    }

    private bool ResolveActionQueue()
    {
        while (actionQueue.ResolveNextKeyword() is {} e)
        {
            if (e is PromptEvent)
            {
                return false;
            }
        }

        return true;
    }

    private IResolutionFrame CreateResolutionFrame(IStep step)
    {
        var resolutionContext = step.CreateAndValidateResolutionContext(gameState, metaGameState, new List<PaymentPayload>(), new List<IAtom>());

        return new ResolutionFrame(resolutionContext, Declare.KeywordInstances(), step.Effects);
    }
}