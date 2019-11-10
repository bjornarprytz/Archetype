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

                if (!cardToPlay.Play(timeline, prompt)) continue;
                
                Resources.ForcePay(cardToPlay.Cost);

                _movesLeft--;
            }

        }


        private Card HandleGetCardToPlay(RequiredAction prompt)
        {
            // TODO: This looks pretty confusing. The ChooseTargets leaves something to be desired.
            Decision result = prompt(new ChooseTargets<Card>(1, Team));

            while (result.Aborted)
            {
                result = prompt(new ChooseTargets<Card>(1, Team));
            }

            return result.ChosenPieces.First() as Card;
        }
    }
}