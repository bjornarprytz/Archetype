using System.Collections.Generic;

namespace Archetype
{
    public class Adventurer : Unit
    {
        public Adventurer(Player owner, string name, int life, int resources, List<Card> cards) 
            : base(owner, cards, name, life, resources, Faction.Player)
        {
        }
    }
}