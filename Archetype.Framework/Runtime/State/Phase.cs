using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;

namespace Archetype.Framework.Runtime.State;

public interface IPhase : IAtom
{
    public IReadOnlyList<IStep> Steps { get; }
    public IReadOnlyList<ActionDescription> AllowedActions { get; }
}

public interface IStep : IActionBlock
{
    public string Name { get; }
}


// TODO: Don't know if this is needed
public record NoPhase : IPhase
{
    public Guid Id => Guid.Empty;
    public IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } = new Dictionary<string, IKeywordInstance>();
    public IDictionary<string, object> State { get; } = new Dictionary<string, object>();
    public IReadOnlyList<IStep> Steps { get; } = new List<IStep>();
    public IReadOnlyList<ActionDescription> AllowedActions { get; } = new List<ActionDescription>();
}

public record NoStep : IStep
{
    public Guid Id => Guid.Empty;
    public IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } = new Dictionary<string, IKeywordInstance>();
    public IDictionary<string, object> State { get; } = new Dictionary<string, object>();
    public IAtom Source { get; } = null;
    public IReadOnlyList<CardTargetDescription> TargetsDescriptors { get; } = new List<CardTargetDescription>();
    public IReadOnlyList<IKeywordInstance> Effects { get; } = new List<IKeywordInstance>();
    public IReadOnlyList<IKeywordInstance> Costs { get; } = new List<IKeywordInstance>();
    public IReadOnlyList<IKeywordInstance> Conditions { get; } = new List<IKeywordInstance>();
    public IReadOnlyList<int> ComputedValues { get; } = new List<int>();
    public void UpdateComputedValues(IRules rules, IGameState gameState)
    {
        throw new NotImplementedException();
    }

    public string Name { get; } = "_NO_STEP_";
}