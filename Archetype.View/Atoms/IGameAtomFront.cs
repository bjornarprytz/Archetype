namespace Archetype.View.Atoms;

public interface IGameAtomFront
{
    IGameAtomFront Owner { get; }
    Guid Guid { get; }
}