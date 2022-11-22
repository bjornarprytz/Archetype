namespace Archetype.Core.Atoms.Zones;

public interface IZone<out T> : IAtom
where T : IAtom
{
    IEnumerable<T> Contents { get; }
}