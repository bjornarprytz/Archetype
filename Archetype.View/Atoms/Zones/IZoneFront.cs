namespace Archetype.View.Atoms.Zones;

public interface IZoneFront : IGameAtomFront
{
    IEnumerable<IGameAtomFront> Contents { get; }
}