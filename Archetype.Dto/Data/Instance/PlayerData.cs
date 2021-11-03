using System.Collections.Generic;
using Archetype.Core.Data.Instance;

namespace Archetype.Core.Data.Composite
{
    public class PlayerData
    {
        public int Resources { get; set; }
        public List<CardInstance> Hand { get; set; } = new();
        public List<CardInstance> DiscardPile { get; set; } = new();
        public List<CardInstance> Deck { get; set; } = new();
    }
}