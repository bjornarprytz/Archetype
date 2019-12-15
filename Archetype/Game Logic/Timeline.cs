using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class Timeline
    {
        public int CurrentTick { get; private set; }
        
        private List<EffectSpan> Effects { get; set; }

        public Timeline(int startTick=0)
        {
            CurrentTick = startTick;
            Effects = new List<EffectSpan>();
        }
        public List<Tick> FutureTicks(int nTicksForward)
        {
            List<Tick> futureTicks = new List<Tick>();

            for (int i = 0; i < nTicksForward; i++)
            {
                futureTicks.Add(new Tick(CurrentTick, Effects));
            }

            return futureTicks;
        }

        internal void AdvanceTime()
        {
            CurrentTick++;
        }


        internal void ResolveEffects(RequiredAction prompt)
        {
            foreach (EffectSpan span in Effects)
            {
                span.ResolveTick(CurrentTick, prompt);
            }
        }

        internal void CommitEffectSpan(EffectSpan span)
        {
            Effects.Add(span);
            span.StartTime = CurrentTick;
        }
    }
}
