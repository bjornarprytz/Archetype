using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class Enemy : Unit
    {
        public List<Card> AvailableMoves => Hand.Cards.Values.ToList();

        public Enemy(string name, ResourcePool resources) : base(name, resources, Faction.Enemy)
        {

        }

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
