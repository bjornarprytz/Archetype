using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class Adventurer : Unit
    {
        public Adventurer(string name, ResourcePool resources, List<Card> cards) 
            : base(name, resources, Faction.Player)
        {
            Deck.PutCardsOnTop(cards);
        }
    }
}