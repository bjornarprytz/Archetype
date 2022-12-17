using Archetype.Core.Proto.PlayingCard;

namespace Archetype.Core.DeckBuilding;

public interface IDeck
{
    public IEnumerable<IProtoPlayingCard> Cards { get; }
}