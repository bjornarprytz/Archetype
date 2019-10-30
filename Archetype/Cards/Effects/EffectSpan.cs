using System;
using System.Collections.Generic;

namespace Archetype
{
    public class EffectSpan
    {
        public int StartTime { get; set; }
        public Dictionary<int, List<Effect>> ChainOfEvents { get; set; }

        public EffectSpan(int currentTick, Dictionary<int, List<Effect>> chainOfEvents)
        {
            if (chainOfEvents.Count == 0) throw new Exception("Empty effect span..");

            StartTime = currentTick;
            ChainOfEvents = chainOfEvents;

            foreach (List<Effect> effects in ChainOfEvents.Values)
            {
                foreach (Effect effect in effects)
                {
                    effect.Whence = this;
                }
            }
        }

        public List<Effect> EffectsAtTick(int tick)
        {
            return ChainOfEvents[tick] ?? new List<Effect>(); ;
        }

        public bool ResolveTick(int tick) // Called by the game state
        {
            if (!ChainOfEvents.ContainsKey(tick)) return true;

            foreach (Effect effect in ChainOfEvents[tick])
            {
                effect.Resolve();
            }

            ChainOfEvents[tick].Clear();
            ChainOfEvents.Remove(tick);

            return ChainOfEvents.Count != 0;
        }

        public void Cancel()
        {
            foreach (int tick in ChainOfEvents.Keys)
            {
                foreach (Effect effect in ChainOfEvents[tick])
                {
                    effect.Cancel();
                }
                ChainOfEvents[tick].Clear();
            }

            ChainOfEvents.Clear();
        }

        private void AddEffect(Effect effect, int when)
        {
            effect.Whence = this;

            if (!ChainOfEvents.ContainsKey(when)) ChainOfEvents[when] = new List<Effect>();

            ChainOfEvents[when].Add(effect);
        }
    }
}