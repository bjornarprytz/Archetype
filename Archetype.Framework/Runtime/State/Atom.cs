namespace Archetype.Framework.Runtime.State;

public abstract class Atom : IAtom
{
    public Guid Id { get; } = Guid.NewGuid();
    public abstract IReadOnlyDictionary<string, string> Characteristics { get; }
}