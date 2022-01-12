using Archetype.View.Primitives;

namespace Archetype.View.Atoms.MetaData;

public record CardMetaData
{
    public string SetName { get; init; }
    public CardRarity Rarity { get; init; }
    public CardColor Color { get; init; }
    public string ImageUri { get; init; }
}