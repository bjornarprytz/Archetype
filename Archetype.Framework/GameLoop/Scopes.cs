using Archetype.Framework.Effects;
using Archetype.Framework.Events;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework.GameLoop;

public interface IGameRoot
{
    // How things are
    IGameState State { get; }
    
    // Player agency
    IEnumerable<IEvent> TakeAction(IActionArgs actionArgs);
}

public interface IScope
{
    Guid Id { get; }
    ScopeLevel Level { get; }
    IScope? Parent { get; }
    IEnumerable<IScope> Nested { get; }
    IEnumerable<IEvent> Events { get; }
    
    IRules Rules { get; }
    IGameState State { get; }
    [PathPart("vars")]
    int? GetVariable(string name);
    void SetVariable(string name, int value);

    internal IScope Exit();
    internal bool IsActonAllowed(IActionArgs actionArgs);
}

public enum ScopeLevel
{
    Game,
    Turn,
    Phase,
    Action,
    Prompt
}

internal class Game(IRules rules) : Scope
{
    private readonly List<Turn> _turns = new();
    private IGameState? _state;
    

    public override IGameState State => _state ?? throw new InvalidOperationException("State not defined. Call Start to initialize the game.");
    public override bool IsActonAllowed(IActionArgs actionArgs)
    {
        return actionArgs switch {
            StartGameArgs _ => true,
            _ => false
        };
    }

    public override IRules Rules => rules;
    
    public override ScopeLevel Level => ScopeLevel.Game;
    public override IScope? Parent => null;
    public override IEnumerable<IScope> Nested => _turns;
    public override IEnumerable<IEvent> Events => Array.Empty<IEvent>();
    
    public Turn NextTurn()
    {
        var turn = new Turn(this);
        
        _turns.Add(turn);
        
        return turn;
    }
}

internal class Turn(Game game) : Scope
{
    private readonly List<Phase> _phases = new();
    
    public override ScopeLevel Level => ScopeLevel.Turn;
    public override IScope? Parent => game;
    public override IEnumerable<IScope> Nested => _phases;
    public override IEnumerable<IEvent> Events => Array.Empty<IEvent>();
    public override bool IsActonAllowed(IActionArgs actionArgs)
    {
        return actionArgs switch {
            EndTurnArgs _ => true,
            _ => false
        };
    }

    public Phase NextPhase()
    {
        var phase = new Phase(this);
        
        _phases.Add(phase);
        
        return phase;
    }
}

internal class Phase(Turn turn) : Scope
{
    private readonly List<Action> _actions = new();
    
    public override ScopeLevel Level => ScopeLevel.Phase;
    public override IScope Parent => turn;
    public override IEnumerable<IScope> Nested => _actions;
    public override IEnumerable<IEvent> Events => Array.Empty<IEvent>();
    public override bool IsActonAllowed(IActionArgs actionArgs)
    {
        return Parent.IsActonAllowed(actionArgs);
    }
    
    public Action NextAction() // TODO: I don't know if these types of methods are a good idea or not
    {
        var action = new Action(this);
        
        _actions.Add(action);
        
        return action;
    }
}

internal class Action(Phase phase) : Scope
{
    public override ScopeLevel Level => ScopeLevel.Action;
    public override IScope Parent => phase;
    public override IEnumerable<IScope> Nested => Array.Empty<IScope>();
    public override IEnumerable<IEvent> Events => Array.Empty<IEvent>();
    public override bool IsActonAllowed(IActionArgs actionArgs)
    {
        return Parent.IsActonAllowed(actionArgs) || actionArgs switch {
            PlayCardArgs args => true,
            _ => false
        };
    }
}


internal abstract class Scope : IScope
{
    private Dictionary<string, int> _variables = new();
    
    public Guid Id { get; } = Guid.NewGuid();
    public bool IsClosed { get; private set; }
    public abstract ScopeLevel Level { get; }
    public abstract IScope? Parent { get; }
    public abstract IEnumerable<IScope> Nested { get; }
    public abstract IEnumerable<IEvent> Events { get; }
    
    public virtual IRules Rules => Parent?.Rules ?? throw new InvalidOperationException("Rules not defined");
    public virtual IGameState State => Parent?.State ?? throw new InvalidOperationException("State not defined");

    public int? GetVariable(string name)
    {
        return _variables.TryGetValue(name, out var value) ? value : Parent?.GetVariable(name);
    }
    
    public void SetVariable(string name, int value)
    {
        _variables[name] = value;
    }
    
    public IScope Exit()
    {
        IsClosed = true;
        
        return Parent ?? throw new InvalidOperationException("Cannot exit the root scope");
    }

    public abstract bool IsActonAllowed(IActionArgs actionArgs);
}