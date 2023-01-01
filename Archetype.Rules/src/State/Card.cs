using Archetype.Core.Atoms.Cards;
using Archetype.Core.Atoms.Zones;
using Archetype.Core.Proto.PlayingCard;

namespace Archetype.Rules.State;

public class Card : Atom, ICard
{
    public IZone<ICard>? CurrentZone { get; set; }
    public IProtoPlayingCard Proto { get; }
}