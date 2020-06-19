using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public abstract class Player : GamePiece
    {
        public List<Unit> Roster { get; set; }
        public int Coin { get; set; }


        public Player(int coin, Faction faction) : base(faction)
        {
            Roster = new List<Unit>();
            Coin = coin;
        }

        public IEnumerable<Unit> ActiveUnits => Roster.Where(unit => unit.IsAlive);
    }
}
