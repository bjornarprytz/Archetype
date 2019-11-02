using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class Adventurer : Unit
    {
        public Adventurer(string name, List<Card> cards) : base(name)
        {
            Deck.PutCardsOnTop(cards);
        }

        public override void TakeTurn(GameState gameState)
        {
            throw new NotImplementedException();
        }
    }
}