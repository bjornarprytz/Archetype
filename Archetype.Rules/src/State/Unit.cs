using Archetype.Core.Atoms.Cards;
using Archetype.Core.Atoms.Zones;
using Archetype.Core.Proto.PlayingCard;

namespace Archetype.Rules.State;

public class Unit : Atom, IUnit
{
    public IProtoPlayingCard Proto { get; }
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public IZone<IUnit>? CurrentZone { get; set; }
}