namespace Archetype.Core.Atoms.Zones;

public interface IZone: IAtom 
{
    IEnumerable<IAtom> Contents { get; }
    void Add(IAtom atom);
    void Remove(IAtom atom);
}

public interface IZone<TAtom> : IZone
    where TAtom : IAtom
{
    new IEnumerable<TAtom> Contents { get; }
    
    void Add(TAtom atom);
    void Remove(TAtom atom);
}