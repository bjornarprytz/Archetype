using System;
using Archetype.Core;

namespace Archetype.Game.Payloads.Metadata
{
    public interface ICardMetaData
    {
        public int Cost { get; set; }
        public string Name { get; set; }

        public CardRarity Rarity { get; set; }
        public CardColor Color { get; set; }

        public string RulesText { get; set; }
        
        public string ImageUri { get; set; }
    }
}