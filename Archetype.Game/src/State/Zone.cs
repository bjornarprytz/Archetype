using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Zones;

namespace Archetype.Game.State;

internal abstract class Zone<TAtom> : Atom, IZone<TAtom> 
    where TAtom : IAtom
{
    protected readonly List<TAtom> Atoms = new ();
    
    protected Zone() { }

    public IEnumerable<TAtom> Contents => Atoms;
}