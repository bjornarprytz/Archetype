using System.Collections.Generic;

namespace Archetype.Core.Data.Composite
{
    public class PlayerData
    {
        public int Resources { get; set; }
        public List<CardData> Hand { get; set; }
        public List<CardData> DiscardPile { get; set; }
    }
}