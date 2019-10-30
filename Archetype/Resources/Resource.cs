using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public struct Resource
    {
        public enum Type
        {
            Life,
            Mana,
            Coin,
        }

        public readonly Type Currency;
        public int Current { get; set; }
        public int Max { get; set; }

        private Resource(Type t, int c, int m)
        {
            Currency = t;
            Current = c;
            Max = m;
        }

        public static Resource Life(int max) { return new Resource(Type.Life, max, max); }
        public static Resource Life(int current, int max) { return new Resource(Type.Life, current, max); }

        public static Resource Mana(int max) { return new Resource(Type.Mana, max, max); }
        public static Resource Mana(int current, int max) { return new Resource(Type.Mana, current, max); }

        public static Resource Coin(int max) { return new Resource(Type.Coin, max, max); }
        public static Resource Coin(int current, int max) { return new Resource(Type.Coin, current, max); }
    }
}
