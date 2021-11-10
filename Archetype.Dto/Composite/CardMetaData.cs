using Archetype.Dto.Simple;

namespace Archetype.Dto.Composite
{
    public class CardMetaData
    {
        public string Name {get; set; }
        public CardRarity Rarity {get; set; }
        public CardColor Color {get; set; }
        public string ImageUri {get; set; }
    }
}