using Archetype.View.Atoms.MetaData;

namespace Archetype.View.Atoms;

public interface IUnitFront : IPieceFront, IGameAtomFront
{
    UnitMetaData BaseMetaData { get; }
        
    int MaxHealth { get; }
    int Health { get; }
        
    int MaxDefense { get; }
    int Defense { get; }
}