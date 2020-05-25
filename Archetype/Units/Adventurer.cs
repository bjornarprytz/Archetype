using System.Collections.Generic;

namespace Archetype
{
    public class Adventurer : Unit
    {
        public Adventurer(string name, int life, int resources, List<Card> cards) 
            : base(name, life, resources, Faction.Player)
        {
            Deck.PutCardsOnTop(cards);
        }
    }
}