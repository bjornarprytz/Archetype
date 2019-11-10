using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class Life : CappedResource
    {
        public Life(int initial, int cap) : base(0, cap)
        {
            Value = initial;
        }
        public Life(int cap) : base (0, cap)
        {
            Value = cap;
        }
    }
}
