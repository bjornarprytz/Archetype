using Archetype.Core.Proto.PlayingCard;

namespace Archetype.Core.DeckBuilding;

public interface IDeck
{
    public string Hash { get; }
    public string Name { get; }
    public IEnumerable<IProtoPlayingCard> Cards { get; }
}