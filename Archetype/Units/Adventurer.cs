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

        internal override void TakeTurn(Timeline timeline, GameState gameState, RequiredAction prompt)
        {
            throw new NotImplementedException();
        }
    }
}