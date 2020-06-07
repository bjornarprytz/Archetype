using System.Collections.Generic;

namespace Archetype
{
    public class Adventurer : Unit
    {
        public Adventurer(Player owner, string name, int life, int resources) 
            : base(owner, name, life, resources)
        {
        }
    }
}