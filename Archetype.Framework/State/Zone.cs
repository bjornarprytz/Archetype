namespace Archetype.Framework.State;

public interface IZone : IAtom
{
    IReadOnlyList<IAtom> Atoms { get; }
    void Add(IAtom atom);
    void Remove(IAtom atom);
}

public interface IOrderedZone : IZone
{
    void Shuffle();
    IAtom? PeekTop();
}

public abstract class Zone : Atom, IZone
{
    protected readonly List<IAtom> InternalAtoms = new();
    public IReadOnlyList<IAtom> Atoms => InternalAtoms;
    public virtual void Add(IAtom atom)
    {
        if (InternalAtoms.Contains(atom))
            throw new InvalidOperationException("Atom already exists in zone.");
        
        InternalAtoms.Add(atom);
    }

    public virtual void Remove(IAtom atom)
    {
        if (!InternalAtoms.Contains(atom))
            throw new InvalidOperationException("Atom does not exist in zone.");
        
        InternalAtoms.Remove(atom);
    }
}