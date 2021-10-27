using System;
using System.Collections.Generic;

namespace Archetype.Core
{
    public class CardData
    {
        public Guid Id { get; set; }
        public int Cost { get; set; }
        public string Name { get; set; }

        public CardRarity Rarity { get; set; }
        public CardColor Color { get; set; }

        public string RulesText { get; set; }
        
        public string ImageUri { get; set; }

        public List<ITargetMetaData> TargetData { get; } = new();
        public List<IEffectMetaData> Effects { get; } = new ();
    }
}
