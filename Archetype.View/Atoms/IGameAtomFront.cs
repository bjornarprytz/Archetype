namespace Archetype.View.Atoms;

public interface IGameAtomFront
{
    string Name { get; }
    IGameAtomFront Owner { get; }
    Guid Guid { get; }
}