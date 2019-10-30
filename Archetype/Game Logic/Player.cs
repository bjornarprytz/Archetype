using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public class Player
    {
        public Dictionary<string, Adventurer> Roster { get; set; }

        public int Coin { get; set; }
        public List<Adventurer> ActiveHeroes
        {
            get
            {
                return Roster.Values.ToList();
            }
        }
    }
}
