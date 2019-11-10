using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class Mana : CappedResource
    {
        public Mana(int initial, int cap) : base(0, cap)
        {
            Value = initial;
        }
        public Mana(int cap) : base(0, cap)
        {
            Value = cap;
        }
    }
}
