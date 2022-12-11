using Archetype.Core.Atoms.Zones;

namespace Archetype.Core.Atoms;

public interface ILocation : IAtom
{
    public string Name { get; }

    public IMap Map { get; }
    
}