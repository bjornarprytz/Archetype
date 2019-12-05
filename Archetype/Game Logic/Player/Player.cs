using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public abstract class Player<UnitType> where UnitType : Unit
    {
        public List<UnitType> Roster { get; set; }
        public ResourcePool Resources { get; set; }


        public Player(ResourcePool resources)
        {
            Roster = new List<UnitType>();
            Resources = resources;
        }

        public IEnumerable<UnitType> ActiveUnits => Roster.Where(unit => unit.IsAlive);
    }
}
