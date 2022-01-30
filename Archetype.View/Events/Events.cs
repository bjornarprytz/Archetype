using Archetype.View.Atoms;

namespace Archetype.View.Events;

public interface IAtomMutation<out T>
    where T : IGameAtomFront
{
    T Atom { get; }
}

public interface IAtomCreated<out T>
    where T : IGameAtomFront
{
    T Atom { get; }
}