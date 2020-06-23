using System.Collections.Generic;

namespace Archetype
{
    public class Adventurer : Unit
    {
        public Adventurer(Player owner, UnitData unitData, IPrompter prompter) 
            : base(owner, unitData, prompter)
        {
        }
    }
}