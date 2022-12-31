using Archetype.Core.Meta;
using Archetype.Core.Proto.PlayingCard;

namespace Archetype.Core.Atoms.Cards;

public interface ICard : 
    IAtom, 
    IZoned<ICard>
{
    public IProtoPlayingCard Proto { get; }
}