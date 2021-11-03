using System.Collections.Generic;

namespace Archetype.Core.Data.Instance
{
    public class PlayerData
    {
        public int Resources { get; set; }
        public List<CardInstance> Hand { get; set; } = new();
        public List<CardInstance> DiscardPile { get; set; } = new();
        public List<CardInstance> Deck { get; set; } = new();
    }
}