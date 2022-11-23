using Archetype.Core.Proto.PlayingCard;

namespace Archetype.Core.DeckBuilding;

public interface ICardCollection
{
    IEnumerable<IProtoPlayingCard> Cards { get; }
}