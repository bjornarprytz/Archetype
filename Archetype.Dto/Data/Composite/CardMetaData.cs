using Archetype.Core;

namespace Archetype.Game.Payloads.Metadata
{
    public class CardMetaData
    {
        public string Name {get; set; }
        public CardRarity Rarity {get; set; }
        public CardColor Color {get; set; }
        public string ImageUri {get; set; }
    }
}