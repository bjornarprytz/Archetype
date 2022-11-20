namespace Archetype.Core.Atoms.Zones;

public interface IZone<T> : IAtom
    where T : IAtom
{
    IEnumerable<T> Contents { get; }

    void Add(T piece);
    void Remove(T piece);
}