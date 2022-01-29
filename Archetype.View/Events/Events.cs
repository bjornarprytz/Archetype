using Archetype.View.Atoms;

namespace Archetype.View.Events;


public interface IAtomMutation
{
    IGameAtomFront Atom { get; }
}
public interface IAtomMutation<out T> : IAtomMutation
    where T : IGameAtomFront
{
    new T Atom { get; }
}

public record AtomMutation<T>(T Atom) : IAtomMutation<T> // TODO: Do more with this (detail which field changed?)
    where T : IGameAtomFront
{
    IGameAtomFront IAtomMutation.Atom => Atom;
}