using Archetype.Core.Proto;

namespace Archetype.Core.DeckBuilding;

public interface IDeck
{
    public IEnumerable<IProtoCard> Cards { get; }
}