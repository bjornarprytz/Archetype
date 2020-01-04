using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class Tick
    {
        public int Timestamp { get; private set; }
        public Stack<Effect> Effects { get; private set; }
        private Tick()
        {
            Effects = new Stack<Effect>();
        }
        public Tick(int timestamp) : this()
        {
            Timestamp = timestamp;
        }

        public void AddEffect(Effect effect)
        {
            Effects.Push(effect);
        }
    }
}
