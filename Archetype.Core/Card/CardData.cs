using System;

namespace Archetype.Core
{
    public record CardData
    {
        public int Cost { get; set; }
        public string Name { get; set; }
        public Guid Id { get; set; }

        public CardRarity Rarity { get; set; }
        public CardColor Color { get; set; }

        public string RulesText { get; set; }
        public string ImagePath { get; set; }
    }
}
