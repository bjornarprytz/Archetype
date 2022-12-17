using Archetype.Core.Atoms;

namespace Archetype.Game.State;

public abstract class Atom : IAtom
{
    public Guid Id { get; } = Guid.NewGuid();

    protected Atom() { }
}