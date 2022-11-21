namespace Archetype.Core.Atoms.Zones;

public interface IZone : IAtom
{
    IEnumerable<ICard> Contents { get; }
}