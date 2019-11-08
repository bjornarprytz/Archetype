using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class Card : GamePiece
    {
        public delegate void ZoneChange(Zone from, Zone to);
        public delegate void BeforePlay();
        public delegate void PlayCard(DecisionPrompt prompt);
        public delegate void AfterPlay();

        public event ZoneChange OnZoneChanged;
        public event BeforePlay OnBeforePlay;
        public event AfterPlay OnAfterPlay;

        public string Name => Template.Name;
        public EffectSpan EffectSpan => Template.EffectSpan;
        public string RulesText { get; set; }
        public CardTemplate Template { get; private set; }
        public bool HasOwner => Owner != null;
        public Unit Owner { get; private set; }
        
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


        public static Card Dummy (string name)
        {
            return new Card(CardTemplate.Dummy(name));
        }

        public Card(CardTemplate template, Zone zone=null)
            : base(zone == null ? Faction.Neutral : zone.Owner.Team)
        {
            Template = template;
            CurrentZone = zone;
        }

        public void MoveTo(Zone newZone)
        {
            if (CurrentZone == newZone) return;

            if (CurrentZone != null) CurrentZone.Out(this);
            if (newZone != null) newZone.Into(this);

            CurrentZone = newZone;
        }

        public virtual bool Play(Timeline timeline, DecisionPrompt prompt)
        {
            foreach(List<Effect> effects in EffectSpan.ChainOfEvents.Values)
            {
                foreach (Effect effect in effects)
                {
                    if (!effect.PromptForTargets(prompt)) return false;

                    effect.Source = Owner;
                }
            }

            OnBeforePlay?.Invoke();

            timeline.CommitEffectSpan(EffectSpan);

            OnAfterPlay?.Invoke();

            return true;
        }

    }
}
