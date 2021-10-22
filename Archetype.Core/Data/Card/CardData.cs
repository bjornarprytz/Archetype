using System;
using System.Collections.Generic;

namespace Archetype.Core
{
    public class CardData
    {
        public int Cost { get; set; }
        public string Name { get; set; }
        public Guid Id { get; set; }

        public CardRarity Rarity { get; set; }
        public CardColor Color { get; set; }

        public string RulesText { get; set; }
        
        public string ImageUri { get; set; }

        public List<CardEffect> Effects { get; } = new List<CardEffect>();
    }
}
