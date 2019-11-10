using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public abstract class CappedResource : Resource
    {
        public override int Value
        {
            get { return _val; }
            set { _val = Math.Min(UpperCap, Math.Max(LowerCap, value)); }
        }
        private int _val;
        public int UpperCap { get; set; }
        public int LowerCap { get; set; }

        public CappedResource(int lowerCap, int upperCap)
        {
            if (lowerCap > upperCap) throw new ArgumentException($"lowerCap [{lowerCap}] must be lower than upperCap [{upperCap}]");

            LowerCap = lowerCap;
            UpperCap = upperCap;
        }
    }
}
