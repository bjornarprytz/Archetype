namespace Archetype.Core.Proto.DeckBuilding;

public interface ICardCollection
{
    IEnumerable<IProtoPlayingCard> Cards { get; }
}