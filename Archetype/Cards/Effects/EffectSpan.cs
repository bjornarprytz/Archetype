using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class EffectSpan : GamePiece
    {
        public int StartTime { get; set; }
        public Dictionary<int, List<Effect>> ChainOfEvents { get; set; }

        public EffectSpan(Dictionary<int, List<Effect>> chainOfEvents) : base()
        {
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
            return ChainOfEvents[tick] ?? new List<Effect>();
        }

        public void ResolveTick(int currentTick, DecisionPrompt prompt) // Called by the game state
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

        internal string GenerateRulesText()
        {
            StringBuilder rulesText = new StringBuilder();

            ChainOfEvents.Keys.ToList()
                .ForEach(tick => ChainOfEvents[tick]
                .ForEach(effect => rulesText.AppendLine($"{tick}: {effect.RulesText}")));

            return rulesText.ToString();
        }

        private void AddEffect(Effect effect, int when)
        {
            effect.Whence = this;

            if (!ChainOfEvents.ContainsKey(when)) ChainOfEvents[when] = new List<Effect>();

            ChainOfEvents[when].Add(effect);
        }
    }
}