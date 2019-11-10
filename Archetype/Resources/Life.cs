using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class Life : Currency
    {
        public Life(int initial, int cap)
        {
            Value = initial;
            UpperCap = cap;
            LowerCap = 0;
        }
        public Life(int cap)
        {
            Value = UpperCap = cap;
            LowerCap = 0;
        }

        public Life() { }
    }
}
