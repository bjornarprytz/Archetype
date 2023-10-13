using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;

namespace Archetype.Framework.Runtime.State;

public interface IPhase : IAtom
{
    public string Name { get; }
    public IReadOnlyList<IStep> Steps { get; }
    public IReadOnlyList<ActionDescription> AllowedActions { get; }
}

public interface IStep : IActionBlock
{
    public string Name { get; }
}

public abstract class Phase : Atom, IPhase
{
    public abstract string Name { get; }
    public abstract IReadOnlyList<IStep> Steps { get; }
    public abstract IReadOnlyList<ActionDescription> AllowedActions { get; }
}

public abstract class Step : IStep
{
    protected Step(IPhase source)
    {
        Source = source;
    }

    public IAtom Source { get; }
    public abstract string Name { get; }
    public abstract IReadOnlyList<IKeywordInstance> Effects { get; }
    
    
    public virtual IReadOnlyList<int> ComputedValues { get; } = ArraySegment<int>.Empty;
    public virtual void UpdateComputedValues(IRules rules, IGameState gameState)
    {
        Console.WriteLine($"Nothing to update in this step ({Name})");
    }
    
    public IReadOnlyList<CardTargetDescription> TargetsDescriptors { get; } = Array.Empty<CardTargetDescription>();
    public IReadOnlyList<IKeywordInstance> Costs { get; } = Array.Empty<IKeywordInstance>();
    public IReadOnlyList<IKeywordInstance> Conditions { get; } = Array.Empty<IKeywordInstance>();
    
}