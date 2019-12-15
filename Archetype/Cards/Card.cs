using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public class Card : GamePiece, ICreateEffects
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
        }

        public void MoveTo(Zone newZone)
        {
            if (CurrentZone == newZone) return;

            if (CurrentZone != null) CurrentZone.Out(this);
            if (newZone != null) newZone.Into(this);

            CurrentZone = newZone;
        }

        internal bool Play(PlayCardArgs args, GameLoop gameLoop)
        {
            if (!args.Valid) return false;


            EffectSpan effectSpan = CreateEffects(args);

            OnBeforePlay?.Invoke();

            gameLoop.Timeline.CommitEffectSpan(effectSpan);

            OnAfterPlay?.Invoke();

            return true;
        }

        public EffectSpan CreateEffects(PlayCardArgs cardArgs)
        {
            EffectSpan effectSpan = new EffectSpan();

            foreach (int tick in _effects.Keys)
            {
                foreach (EffectArgs effectArg in cardArgs.EffectArgs)
                {
                    effectSpan.AddEffect(tick, effectArg.Effect.CreateEffect(effectArg));
                }
            }

            return effectSpan;
        }
    }
}
