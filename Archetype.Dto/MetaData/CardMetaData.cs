using Archetype.Dto.Simple;

namespace Archetype.Dto.MetaData
{
    public record CardMetaData
    {
        public string Name { get; init; }
        public string SetName { get; init; }
        public CardRarity Rarity {get; init; }
        public CardColor Color {get; init; }
        public string ImageUri {get; init; }
    }
}