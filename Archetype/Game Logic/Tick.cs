using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class Tick
    {
        public int Timestamp { get; set; }
        public List<Effect> Effects { get; private set; }

        public Tick(int timestamp, List<EffectSpan> allKnownSpans)
        {
            Timestamp = timestamp;

            UpdateEffects(allKnownSpans);
        }

        public void UpdateEffects(List<EffectSpan> allKnownSpans)
        {
            Effects = new List<Effect>();

            foreach (EffectSpan span in allKnownSpans)
            {
                Effects.AddRange(span.EffectsAtTick(Timestamp));
            }
        }
    }
}
