using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class Adventurer : Unit
    {
        private int _movesLeft = 3; // TODO: Find some good logic to determine whether there's more to do for this unit.

        public Adventurer(string name, ResourcePool resources, List<Card> cards) 
            : base(name, resources, Faction.Player)
        {
            Deck.PutCardsOnTop(cards);
        }

        internal override void TakeTurn(Timeline timeline, GameState gameState, RequiredAction prompt)
        {
            // TODO: Take into account empty hands, no resources, skipping turn etc.
            while (_movesLeft > 0)
            {
                Card cardToPlay = HandleGetCardToPlay(prompt);

                if (!Resources.CanAfford(cardToPlay.Cost)) continue;

                if (!cardToPlay.Play(timeline, gameState, prompt)) continue;
                
                Resources.ForcePay(cardToPlay.Cost);

                _movesLeft--;
            }

        }


        private Card HandleGetCardToPlay(RequiredAction prompt)
        {
            Choose<Card> choose = new Choose<Card>(1, Hand);

            prompt(choose);

            while (choose.Aborted)
            {
                choose = new Choose<Card>(1, Hand);
                prompt(choose);
            }

            return choose.Choices.First() as Card;
        }
    }
}