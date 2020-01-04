using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class EffectSpan : GamePiece
    {
        public int StartTime { get; set; }
        public Dictionary<int, Effect> ChainOfEvents { get; set; }

        public EffectSpan(Dictionary<int, Effect> chainOfEvents=null) : base()
        {

            ChainOfEvents = chainOfEvents ?? new Dictionary<int, Effect>(); ;
        }

        public void Cancel()
        {
            foreach (int tick in ChainOfEvents.Keys)
            {
                ChainOfEvents[tick].Cancel();
            }

            ChainOfEvents.Clear();
        }

        public void AddEffect(int when, Effect effect)
        {
            ChainOfEvents[when] = effect;
        }

    }
}