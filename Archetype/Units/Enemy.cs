using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class Enemy : Unit
    {
        List<Move> MoveSet { get; set; }
        List<Move> AvailableMoves { get; set; }

        public Enemy(string name) : base(name)
        {
            MoveSet = new List<Move>();
            AvailableMoves = new List<Move>();
        }

        public void MakeMove(Move move)
        {
            // Make moves based on game state
        }

        public override void TakeTurn(GameState gameState)
        {
            if (AvailableMoves.Count == 0)
            {
                Console.WriteLine($"No available moves for <{Name}>!");
                return;
            }

            int maxVal = -1;
            Move chosenMove = AvailableMoves[0];
            foreach (Move move in AvailableMoves)
            {
                int moveVal = Evaluate(move, gameState);
                if (moveVal > maxVal)
                {
                    maxVal = moveVal;
                    chosenMove = move;
                }
            }

            MakeMove(chosenMove);
        }

        private int Evaluate(Move move, GameState gameState)
        {
            // TODO: Do smart things

            return 0;
        }
    }
}
