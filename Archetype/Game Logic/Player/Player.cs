using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public abstract class Player<UnitType> where UnitType : Unit
    {
        public List<UnitType> Roster { get; set; }
        public int Coin { get; set; }


        public Player(int coin)
        {
            Roster = new List<UnitType>();
            Coin = coin;
        }

        public IEnumerable<UnitType> ActiveUnits => Roster.Where(unit => unit.IsAlive);
    }
}
