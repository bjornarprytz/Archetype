using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public class Player
    {
        public Dictionary<string, Adventurer> Roster { get; set; }

        public ResourcePool Resources { get; set; }

        public Player(ResourcePool resources)
        {
            Roster = new Dictionary<string, Adventurer>();
            Resources = resources;
        }

        public List<Adventurer> ActiveHeroes
        {
            get
            {
                return Roster.Values.ToList();
            }
        }
    }
}
