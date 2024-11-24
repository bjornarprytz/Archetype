namespace Archetype.Framework.State;

public interface IZone : IAtom
{
    int AtomCount { get; }
    
    [PathPart("atoms")]
    IAtom[] GetAtoms();
    
    bool AddAtom(IAtom atom);
    bool RemoveAtom(IAtom atom);
}

public class Zone : Atom, IZone
{
    private Dictionary<Guid, IAtom> _atoms = new();
    
    public int AtomCount => _atoms.Count;
    public IAtom[] GetAtoms()
    {
        return _atoms.Values.ToArray();
    }

    public bool AddAtom(IAtom atom)
    {
        return _atoms.TryAdd(atom.Id, atom);
    }

    public bool RemoveAtom(IAtom atom)
    {
        return _atoms.Remove(atom.Id);
    }
}