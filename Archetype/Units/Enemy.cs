using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class Enemy : Unit
    {
        public List<Card> AvailableMoves => Hand.Cards.Values.ToList();

        public Enemy(string name) : base(name, Faction.Enemy)
        {

        }

        public void MakeMove(Card move)
        {
            // Make moves based on game state
        }

        public override void TakeTurn(Timeline timeline, GameState gameState, DecisionPrompt prompt)
        {
            if (Hand.IsEmpty)
            {
                Console.WriteLine($"No available moves for <{Name}>!");
                return;
            }

            int maxVal = -1;
            Card chosenMove = AvailableMoves[0];
            foreach (Card move in AvailableMoves)
            {
                int moveVal = Evaluate(move, gameState);
                if (moveVal > maxVal)
                {
                    maxVal = moveVal;
                    chosenMove = move;
                }
            }

            chosenMove.Play(timeline, prompt);
        }

        private int Evaluate(Card move, GameState gameState)
        {
            // TODO: Do smart things

            return 0;
        }
    }
}
