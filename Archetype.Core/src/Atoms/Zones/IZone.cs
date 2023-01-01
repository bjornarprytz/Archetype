namespace Archetype.Core.Atoms.Zones;

public interface IZone<TAtom> : IAtom
    where TAtom : IAtom
{
    IEnumerable<TAtom> Contents { get; }
    
    void Add(TAtom atom);
    void Remove(TAtom atom);
}