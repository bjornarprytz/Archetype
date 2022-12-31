using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Zones;

namespace Archetype.Core.Meta;

public interface IZoned<TAtom> 
    where TAtom : IAtom
{
    IZone<TAtom>? CurrentZone { get; set; }
}

public interface IHealth
{
    int CurrentHealth { get; set; }
    int MaxHealth { get; set; }
}