using System.Collections.Generic;

namespace Archetype.Core.Enemy
{
    public class EnemyData
    {
        public string Name {get; set;}
        public string ImageUri {get; set;}
        public int Health { get; set; }
        public List<CardData> Cards {get; set;}
        
        
    }
}