namespace Archetype.Core.Atoms.Zones;

public interface IZone<T> : IAtom
    where T : IAtom
{
    IEnumerable<T> Contents { get; }
    
    void Add(T atom);
    void Remove(T atom);
}