using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public class Card : GamePiece
    {
        public delegate void ZoneChange(Zone from, Zone to);
        public delegate void BeforePlay();
        public delegate void PlayCard(RequiredAction prompt);
        public delegate void AfterPlay();

        public event ZoneChange OnZoneChanged;
        public event BeforePlay OnBeforePlay;
        public event AfterPlay OnAfterPlay;

        public string Name { get; private set; }
        public CompoundPayment Cost { get; set; }
        public string RulesText { get; private set; }
        public bool HasOwner => Owner != null;
        public Unit Owner { get; private set; }
        private Dictionary<int, List<EffectTemplate>> _effects;
        
        public Zone CurrentZone
        {
            get { return currZone; }
            private set
            {
                Zone previousZone = currZone;
                currZone = value;
                Owner = currZone?.Owner;
                OnZoneChanged?.Invoke(previousZone, currZone);
            }
        }
        private Zone currZone;

        internal Card(string name, CompoundPayment cost, Dictionary<int, List<EffectTemplate>> effects=null)
        {
            Name = name;
            Cost = cost;
            _effects = effects ?? new Dictionary<int, List<EffectTemplate>>();
            RulesText = GenerateRulesText(_effects);
        }

        public void MoveTo(Zone newZone)
        {
            if (CurrentZone == newZone) return;

            if (CurrentZone != null) CurrentZone.Out(this);
            if (newZone != null) newZone.Into(this);

            CurrentZone = newZone;
        }

        public virtual bool Play(Timeline timeline, RequiredAction prompt)
        {
            EffectSpan effectSpan = PromptForTargets(prompt);
            if (effectSpan == null) return false;

            OnBeforePlay?.Invoke();

            timeline.CommitEffectSpan(effectSpan);

            OnAfterPlay?.Invoke();

            return true;
        }

        private EffectSpan PromptForTargets(RequiredAction prompt)
        {
            EffectSpan effectSpan = new EffectSpan();

            foreach (int tick in _effects.Keys)
            {
                foreach (EffectTemplate effectTemplate in _effects[tick])
                {
                    Decision result = prompt(effectTemplate.Requirements);

                    if (result.Aborted) return null; // TODO: Find a better way to signal aborted prompts

                    effectSpan.AddEffect(tick, effectTemplate.CreateEffect(Owner, result)); // TODO: Make sure Owner can't be null at this point
                }
            }

            return effectSpan;
        }

        private string GenerateRulesText(Dictionary<int, List<EffectTemplate>> effects)
        {
            StringBuilder rulesText = new StringBuilder();

            effects.Keys.OrderBy(tick => tick).ToList()
                .ForEach(tick => effects[tick]
                .ForEach(effect => rulesText.AppendLine($"{tick}: {effect.RulesText}")));

            return rulesText.ToString();
        }

    }
}
