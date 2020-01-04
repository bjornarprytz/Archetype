using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class Timeline
    {
        public Tick CurrentTick => Ticks.ContainsKey(_currTick) ? Ticks[_currTick] : new Tick(_currTick);
        private int _currTick;
        
        private List<EffectSpan> Effects { get; set; }

        private Dictionary<int, Tick> Ticks { get; set; }

        public Timeline(int startTick=0)
        {
            _currTick = startTick;
            Effects = new List<EffectSpan>();
        }
        public IEnumerable<Tick> FutureTicks(int nTicksForward) => Ticks.Values.Where(t => t.Timestamp > _currTick && t.Timestamp < _currTick + nTicksForward);

        internal void AdvanceTime()
        {
            _currTick++;
        }


        internal void ResolveEffects(RequiredAction prompt)
        {
            while(CurrentTick.Effects.Count > 0)
            {
                CurrentTick.Effects.Pop().Resolve(prompt);
            }
        }

        internal void CommitEffectSpan(EffectSpan span)
        {
            Effects.Add(span);
            span.StartTime = _currTick;
            foreach(int relativeTimeStamp in span.ChainOfEvents.Keys)
            {
                int absTimeStamp = relativeTimeStamp + _currTick;

                if (!Ticks.ContainsKey(relativeTimeStamp)) Ticks.Add(absTimeStamp, new Tick(absTimeStamp));

                Ticks[relativeTimeStamp].AddEffect(span.ChainOfEvents[relativeTimeStamp]);
            }
        }
    }
}
