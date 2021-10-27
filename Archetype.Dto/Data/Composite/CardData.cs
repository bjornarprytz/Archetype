using System;
using System.Collections.Generic;
using Archetype.Core.Data.Composite;

namespace Archetype.Core
{
    public class CardData
    {
        public Guid Id {get; set; }
        public int Cost {get; set; }
        public string Name {get; set; }
        public CardRarity Rarity {get; set; }
        public CardColor Color {get; set; }
        public string RulesText {get; set; }
        public string ImageUri {get; set; }
        public List<TargetData> Targets { get; set; } = new();
        public List<EffectData> Effects { get; set; } = new();

    }
}
