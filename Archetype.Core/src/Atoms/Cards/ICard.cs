using Archetype.Core.Atoms.Zones;
using Archetype.Core.Effects;
using Archetype.Core.Meta;
using Archetype.Core.Proto.PlayingCard;

namespace Archetype.Core.Atoms.Cards;

public interface ICard : IAtom
{
    public IProtoPlayingCard Proto { get; }
    public IZone<ICard> CurrentZone { get; }


    [Keyword("Move card to {0}")]
    public IResult MoveTo(IZone<ICard> newZone);
}