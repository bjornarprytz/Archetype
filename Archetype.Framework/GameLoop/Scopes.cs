using Archetype.Framework.Effects;
using Archetype.Framework.Events;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework.GameLoop;

public interface IGameRoot
{
    // How things are
    IGameState State { get; }
    
    // Available cards (proto data)
    ICardPool CardPool { get; }
    
    // Player agency
    IEnumerable<IEvent> TakeAction(IActionArgs actionArgs);
}

public interface IScope
{
    Guid Id { get; }
    IScope? Parent { get; }
    IScope? CurrentSubScope { get; }
    IEnumerable<IScope> Nested { get; }
    IEnumerable<IEvent> Events { get; }
    
    [PathPart("vars")]
    int? GetVariable(string name);
    void SetVariable(string name, int value);
    

    void AddEvent(IEvent @event);
    bool IsClosed { get; }
    void Exit();
}

internal class Game() : Scope(null)
{
    public IScope GetEdgeScope()
    {
        IScope scope = this;
        var loopDetector = new HashSet<IScope>(){ scope };
        
        
        while (scope.CurrentSubScope != null)
        {
            scope = scope.CurrentSubScope;
            
            if (!loopDetector.Add(scope))
            {
                throw new InvalidOperationException("Scope loop detected");
            }
        }

        return scope;
    }
}

internal class Turn(Game game) : Scope(game)
{
    
}

internal class Phase(Turn turn) : Scope(turn)
{

}

internal class GameAction(Phase phase) : Scope(phase)
{
}

internal class Prompt(GameAction gameAction) : Scope(gameAction)
{
    
}


internal abstract class Scope(IScope? parent) : IScope
{
    private Dictionary<string, int> _variables = new();
    private List<IEvent> _events = new();
    private List<IScope> _nested = new();
    
    public Guid Id { get; } = Guid.NewGuid();
    public bool IsClosed { get; private set; }
    public IScope? Parent => parent;
    public IScope? CurrentSubScope { get; private set; } 
    public IEnumerable<IScope> Nested => _nested;
    public IEnumerable<IEvent> Events => _events;

    public int? GetVariable(string name)
    {
        return _variables.TryGetValue(name, out var value) ? value : Parent?.GetVariable(name);
    }
    
    public void SetVariable(string name, int value)
    {
        _variables[name] = value;
    }
    
    public void AddEvent(IEvent @event)
    {
        _events.Add(@event);
    }
    
    public void EnterSubScope(IScope subScope)
    {
        if (IsClosed)
        {
            throw new InvalidOperationException("Scope is already closed");
        }
        
        if (CurrentSubScope != null)
        {
            throw new InvalidOperationException("Sub scope is already set");
        }
        
        CurrentSubScope = subScope;
    }
    
    public void Exit()
    {
        if (IsClosed)
        {
            throw new InvalidOperationException("Scope is already closed");
        }

        if (CurrentSubScope != null)
        {
            CurrentSubScope.Exit();
            _nested.Add(CurrentSubScope);
        }
        
        CurrentSubScope = null;

        IsClosed = true;
    }
}