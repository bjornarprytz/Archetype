using Archetype.BasicRules.Primitives;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime.Actions;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Runtime.Implementation;

public class GameLoop : IGameLoop
{
    private readonly IGameRoot _gameRoot;
    private readonly IActionQueue _actionQueue;
    private readonly IReadOnlyList<IPhase> _phases;
    
    private readonly Queue<IPhase> _remainingPhases = new();
    private readonly Queue<IStep> _remainingSteps = new();

    public GameLoop(IGameRoot gameRoot)
    {
        _gameRoot = gameRoot;
        _actionQueue = _gameRoot.Infrastructure.ActionQueue;
        _phases = _gameRoot.MetaGameState.Rules.Phases;
        
        // TODO: Maybe move this validation elsewhere?
        if (_phases.Count == 0)
            throw new InvalidOperationException("No phases defined");

        if (!_phases.Any(p => p.AllowedActions.Count > 0))
            throw new InvalidOperationException("None of the phases have any allowed actions");
        
        if (!_phases.Where(p => p.AllowedActions.Count > 0).All(p => p.AllowedActions.Any(a => a.Type == ActionType.PassTurn)))
            throw new InvalidOperationException("Not all phases have a pass turn action");
    }

    public IPhase CurrentPhase { get; private set; } = new NoPhase();

    public IGameAPI Advance()
    {
        // TODO: Check victory conditions now and then (when?)
        
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

            CurrentPhase = NextPhase();
        }
    }

    public IGameAPI EndPhase()
    {
        CurrentPhase = NextPhase();

        return Advance();
    }

    private IPhase NextPhase()
    {
        if (_remainingPhases.TryDequeue(out var nextPhase)) return nextPhase;
        
        // Next turn
        foreach (var phase in _phases)
        {
            _remainingPhases.Enqueue(phase);
        }

        return _remainingPhases.Dequeue();
    }

    private bool ResolveSteps()
    {
        while (_remainingSteps.TryDequeue(out var step))
        {
            _actionQueue.Push(CreateResolutionFrame(step));
                
            if (!ResolveActionQueue())
            {
                return false;
            }
        }

        return true;
    }

    private bool ResolveActionQueue()
    {
        while (_actionQueue.ResolveNextKeyword() is {} e)
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
        var resolutionContext = step.CreateAndValidateResolutionContext(_gameRoot, new List<PaymentPayload>(), new List<IAtom>());

        return new ResolutionFrame(resolutionContext, Declare.KeywordInstances(), step.Effects);
    }
}