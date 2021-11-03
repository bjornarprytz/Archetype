using Archetype.Core.Data.Simple;

namespace Archetype.Core.Data.Composite
{
    public class CardMetaData
    {
        public string Name {get; set; }
        public CardRarity Rarity {get; set; }
        public CardColor Color {get; set; }
        public string ImageUri {get; set; }
    }
}