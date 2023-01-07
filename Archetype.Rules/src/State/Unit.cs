using Archetype.Core.Atoms.Cards;
using Archetype.Core.Atoms.Zones;
using Archetype.Core.Proto;

namespace Archetype.Rules.State;

public class Unit : Atom, IUnit
{
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public IZone? CurrentZone { get; set; }
    public IProtoCard Proto { get; }
}