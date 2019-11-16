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
            // TODO: Take into account skipping turn etc.
            while (HasMovesAvailable)
            {
                Card cardToPlay = HandleGetCardToPlay(prompt);

                if (!Resources.CanAfford(cardToPlay.Cost)) continue;

                if (!cardToPlay.Play(timeline, gameState, prompt)) continue;
                
                Resources.ForcePay(cardToPlay.Cost);
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