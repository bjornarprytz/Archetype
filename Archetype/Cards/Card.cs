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

        public string Name { get; set; }
        public string RulesText { get; set; }
        public EffectSpan Effects { get; set; }

        
        public Zone CurrentZone
        {
            get { return currZone; }
            private set
            {
                Zone previousZone = currZone;
                currZone = value;
                OnZoneChanged?.Invoke(previousZone, currZone);
            }
        }
        private Zone currZone;

        public static Card Dummy (string name)
        {
            return new Card(name, new EffectSpan(new Dictionary<int, List<Effect>>()));
        }

        public Card(string name, EffectSpan effects, Zone zone=null)
            : base(zone == null ? Faction.Neutral : zone.Owner.Team)
        {
            Name = name;
            CurrentZone = zone;
        }

        public void MoveTo(Zone newZone)
        {
            if (CurrentZone == newZone) return;

            if (CurrentZone != null) CurrentZone.Out(this);
            if (newZone != null) newZone.Into(this);

            CurrentZone = newZone;
        }

        public virtual void Play(Timeline timeline, DecisionPrompt prompt)
        {
            foreach(List<Effect> effects in Effects.ChainOfEvents.Values)
            {
                foreach (Effect effecet in effects)
                {
                    effecet.PromptForTargets(prompt);
                }
            }


            OnBeforePlay?.Invoke();
            


            OnAfterPlay?.Invoke();
        }

    }
}
