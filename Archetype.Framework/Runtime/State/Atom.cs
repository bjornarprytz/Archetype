using Archetype.Framework.Proto;

namespace Archetype.Framework.Runtime.State;

public abstract class Atom : IAtom
{
    public Guid Id { get; } = Guid.NewGuid();
    public abstract IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; }
    public IDictionary<string, object> State { get; } = new Dictionary<string, object>();
}