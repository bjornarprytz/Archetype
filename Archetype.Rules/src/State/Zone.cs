using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Zones;

namespace Archetype.Rules.State;

public abstract class Zone<TAtom> : Atom, IZone<TAtom> 
    where TAtom : IAtom
{
    protected readonly List<TAtom> Atoms = new ();

    public IEnumerable<TAtom> Contents => Atoms;
    IEnumerable<IAtom> IZone.Contents => Atoms as IEnumerable<IAtom> ?? Array.Empty<IAtom>();
    public void Add(IAtom atom)
    {
        if (atom is not TAtom tAtom)
        {
            throw new ArgumentException($"Cannot add {atom} to {this}. Expected atom of (sub)type {typeof(TAtom)}, got {atom.GetType()}");
        }
        
        Add(tAtom);
    }

    public void Remove(IAtom atom)
    {
        if (atom is not TAtom tAtom)
        {
            throw new ArgumentException($"Cannot remove {atom} from {this}. Expected atom of (sub)type {typeof(TAtom)}, got {atom.GetType()}");
        }
        
        Remove(tAtom);
    }

    public void Add(TAtom atom)
    {
        Atoms.Add(atom);
        OnAtomAdded(atom);
    }

    public void Remove(TAtom atom)
    {
        Atoms.Remove(atom);
        OnAtomRemoved(atom);
    }
    
    protected virtual void OnAtomAdded(TAtom atom) { }
    protected virtual void OnAtomRemoved(TAtom atom) { }
}