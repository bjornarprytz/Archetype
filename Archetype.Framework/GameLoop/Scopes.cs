using Archetype.Framework.Effects;
using Archetype.Framework.Events;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework.GameLoop;

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

    public IScope Exit();
}

public enum ScopeLevel
{
    Game,
    Turn,
    Phase,
    Action,
    Prompt
}

public class Game(IRules rules) : Scope
{
    private readonly List<Turn> _turns = new();
    private IGameState? _state;
    

    public override IGameState State => _state ?? throw new InvalidOperationException("State not defined. Call Start to initialize the game.");
    public override IRules Rules => rules;
    
    public override ScopeLevel Level => ScopeLevel.Game;
    public override IScope? Parent => null;
    public override IEnumerable<IScope> Nested => _turns;
    public override IEnumerable<IEvent> Events => Array.Empty<IEvent>();
    
    public IGameLoop Start(int seed)
    {
        _state = Rules.InitializeState(seed);
        
        return new GameLoop(this);
    }
    
    public Turn NextTurn()
    {
        var turn = new Turn(this);
        
        _turns.Add(turn);
        
        return turn;
    }
}

public class Turn(Game game) : Scope
{
    private readonly List<Phase> _phases = new();
    
    public override ScopeLevel Level => ScopeLevel.Turn;
    public override IScope? Parent => game;
    public override IEnumerable<IScope> Nested => _phases;
    public override IEnumerable<IEvent> Events => Array.Empty<IEvent>();
    
    public Phase NextPhase()
    {
        var phase = new Phase(this);
        
        _phases.Add(phase);
        
        return phase;
    }
}

public class Phase(Turn turn) : Scope
{
    private readonly List<Action> _actions = new();
    
    public override ScopeLevel Level => ScopeLevel.Phase;
    public override IScope Parent => turn;
    public override IEnumerable<IScope> Nested => _actions;
    public override IEnumerable<IEvent> Events => Array.Empty<IEvent>();
    
    public IEnumerable<IEffectResult> Resolve(IResolutionContext context)
    {
        throw new NotImplementedException();
    }
}

public class Action(Phase phase) : Scope
{
    public override ScopeLevel Level => ScopeLevel.Action;
    public override IScope? Parent => phase;
    public override IEnumerable<IScope> Nested => Array.Empty<IScope>();
    public override IEnumerable<IEvent> Events => Array.Empty<IEvent>();
}


public abstract class Scope : IScope
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
}