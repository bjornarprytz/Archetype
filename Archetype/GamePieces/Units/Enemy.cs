using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class Enemy : Unit
    {
        public List<Card> AvailableMoves => Hand.Cards.Values.ToList();

        public Enemy(Player owner, UnitData unitData, IPrompter prompter) : base(owner, unitData, prompter) { }

        public void MakeMove(Card move)
        {
            // Make moves based on game state
        }


        private int Evaluate(Card move, GameState gameState)
        {
            // TODO: Do smart things

            return 0;
        }
    }
}
