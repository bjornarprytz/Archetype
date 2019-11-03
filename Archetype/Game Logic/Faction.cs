using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    [Flags]
    public enum Faction
    {
        Neutral = 1 << 0,
        Player  = 1 << 1,
        Enemy   = 1 << 2,


        Any     = Neutral | Player | Enemy,
    }
}
