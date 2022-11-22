namespace Archetype.Core.Proto.DeckBuilding;

public interface IDeck
{
    public string Hash { get; }
    public string Name { get; }
    public IEnumerable<IProtoPlayingCard> Cards { get; }
}