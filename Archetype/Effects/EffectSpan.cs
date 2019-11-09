using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class EffectSpan : GamePiece
    {
        public int StartTime { get; set; }
        public Dictionary<int, List<Effect>> ChainOfEvents { get; set; }

        public EffectSpan(Dictionary<int, List<Effect>> chainOfEvents=null) : base()
        {

            ChainOfEvents = chainOfEvents ?? new Dictionary<int, List<Effect>>(); ;
        }

        public List<Effect> EffectsAtTick(int tick)
        {
            return ChainOfEvents[tick] ?? new List<Effect>();
        }

        public void ResolveTick(int currentTick, RequiredAction prompt)
        {
            int relativeTick = currentTick - StartTime;

            if (!ChainOfEvents.ContainsKey(relativeTick)) return;

            foreach (Effect effect in ChainOfEvents[relativeTick])
            {
                effect.Resolve(prompt);
            }
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

        public void AddEffect(int when, Effect effect)
        {
            if (!ChainOfEvents.ContainsKey(when)) ChainOfEvents[when] = new List<Effect>();

            ChainOfEvents[when].Add(effect);
        }

    }
}