using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public class Card : GamePiece, ICreateEffects, IOwned<Unit>
    {
        public delegate void ZoneChange(Zone<Card> from, Zone<Card> to);
        public delegate void BeforePlay();
        public delegate void AfterPlay();

        public event ZoneChange OnZoneChanged;
        public event BeforePlay OnBeforePlay;
        public event AfterPlay OnAfterPlay;

        public string Name { get; private set; }
        public CompoundPayment Cost { get; set; }

        public Unit Owner { get; set; }
        private Dictionary<int, List<EffectTemplate>> _effects;
        
        public Zone<Card> CurrentZone
        {
            get { return currZone; }
            private set
            {
                Zone<Card> previousZone = currZone;
                currZone = value;
                OnZoneChanged?.Invoke(previousZone, currZone);
            }
        }
        private Zone<Card> currZone;

        internal Card(string name, CompoundPayment cost, Dictionary<int, List<EffectTemplate>> effects=null)
        {
            Name = name;
            Cost = cost;
            _effects = effects ?? new Dictionary<int, List<EffectTemplate>>();
        }

        public void MoveTo(Zone<Card> newZone)
        {
            if (CurrentZone == newZone) return;

            if (CurrentZone != null) CurrentZone.Out(this);
            if (newZone != null) newZone.Into(this);

            CurrentZone = newZone;
        }

        internal bool Play(PlayCardArgs args, GameState gameState)
        {
            if (!args.Valid) return false;


            EffectSpan effectSpan = CreateEffects(args);

            OnBeforePlay?.Invoke();

            gameState.Timeline.CommitEffectSpan(effectSpan);

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
