using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class Adventurer : Unit
    {
        public Adventurer(string name, List<Card> cards) : base(name, Faction.Player)
        {
            Deck.PutCardsOnTop(cards);
        }

        public override void TakeTurn(Timeline timeline, GameState gameState, DecisionPrompt prompt)
        {
            throw new NotImplementedException();
        }
    }
}