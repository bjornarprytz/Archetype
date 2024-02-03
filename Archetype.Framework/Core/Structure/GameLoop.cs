using System.Collections.Immutable;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Interface;
using Archetype.Framework.State;

namespace Archetype.Framework.Core.Structure;

public interface IGameLoop
{
    IPhase? CurrentPhase { get; }
    IGameApi Advance();
    IGameApi EndPhase();
}

public class GameLoop(IActionQueue actionQueue, IGameState gameState, IMetaGameState metaGameState)
    : IGameLoop
{
    private readonly Queue<IPhase> _remainingPhases = new();

    public IPhase? CurrentPhase { get; private set; }

    public IGameApi Advance()
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
            
            if (CurrentPhase.AllowedActions.Count != 0)
            {
                return new GameApi(CurrentPhase.AllowedActions);
            }

            CurrentPhase = MountNextPhase();
        }
    }

    public IGameApi EndPhase()
    {
        CurrentPhase = MountNextPhase();

        return Advance();
    }

    private IPhase MountNextPhase()
    {
        if (_remainingPhases.Count != 0) return _remainingPhases.Dequeue();
        
        // Next turn
        foreach (var phase in GetTurnSequence())
        {
            _remainingPhases.Enqueue(phase);
        }

        return _remainingPhases.Dequeue();;
    }

    private bool ResolveActionQueue()
    {
        actionQueue.Push(CreateResolutionFrame());
         
        while (actionQueue.ResolveNextKeyword() is IEffectEvent e)
        {
            if (e.Result is IPromptDescription )
            {
                return false;
            }
        }

        return true;
    }

    private IResolutionFrame CreateResolutionFrame()
    {
        if (CurrentPhase == null)
        {
            throw new InvalidOperationException("No current phase");
        }
        
        var resolutionContext = new ResolutionContext(metaGameState, gameState, CurrentPhase);

        return new ResolutionFrame(resolutionContext, CurrentPhase.Steps);
    }
    
    private IReadOnlyList<IPhase> GetTurnSequence() => metaGameState.ProtoData.TurnSequence ?? throw new InvalidOperationException("No turn sequence defined");
}