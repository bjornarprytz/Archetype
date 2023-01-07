using Archetype.Core.Proto;

namespace Archetype.Core.DeckBuilding;

public interface ICardCollection
{
    IEnumerable<IProtoCard> Cards { get; }
}