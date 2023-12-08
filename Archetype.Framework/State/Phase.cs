using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Design;
using Archetype.Framework.Interface;

namespace Archetype.Framework.State;

public interface IPhase : IAtom
{
    public string Name { get; }
    public IReadOnlyList<IKeywordInstance> Steps { get; }
    public IReadOnlyList<ActionDescription> AllowedActions { get; }
}


public abstract class Phase : Atom, IPhase
{
    public override IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } =
        new Dictionary<string, IKeywordInstance>();
    public abstract string Name { get; }
    public abstract IReadOnlyList<IKeywordInstance> Steps { get; }
    public abstract IReadOnlyList<ActionDescription> AllowedActions { get; }
}
