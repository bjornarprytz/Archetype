using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public abstract class Card : GamePiece, IOwned<Unit>, IZoned<Card>, IHoldCounters
    {
        public event ZoneChange<Card> OnZoneChanged;

        public delegate void BeforePlay();
        public delegate void AfterPlay();

        public event BeforePlay OnBeforePlay;
        public event AfterPlay OnAfterPlay;

        public string Name { get; protected set; }
        public int Cost { get; set; }

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

        public Unit Owner { get; set; }

        protected List<EffectTemplate> _effectTemplates;
        public TypeDictionary<Counter> ActiveCounters { get; private set; }

        internal Card()
        {
            ActiveCounters = new TypeDictionary<Counter>();
            _effectTemplates = CreateEffectList();
        }

        public void MoveTo(Zone<Card> newZone)
        {
            // TODO: Move this implementation to the IZoned interface (available in C# 8.0)

            if (CurrentZone == newZone) return;

            if (CurrentZone != null) CurrentZone.Out(this);
            if (newZone != null) newZone.Into(this);

            CurrentZone = newZone;
        }

        public IEnumerable<EffectArgs> GetArgs(GameState gameState) => _effectTemplates.Select(e => e.Args(Owner, gameState));
        
        public virtual void Apply<T>(T counter) where T : Counter
        {
            if (ActiveCounters.Has<T>())
                ActiveCounters.Get<T>().Combine(counter);
            else
                ActiveCounters.Set<T>(counter);
        }
        public void Remove<T>() where T : Counter
        {
            if (ActiveCounters.Has<T>()) ActiveCounters.Remove<T>();
        }

        internal bool Play(PlayCardArgs args, GameState gameState)
        {
            if (!args.Valid) return false;

            OnBeforePlay?.Invoke();

            args.EffectArgs.Select(a => a.Effect.CreateEffect(a));

            foreach (Effect effect in args.CreateEffects())
            {
                effect.Resolve(gameState);
            }

            OnAfterPlay?.Invoke();

            MoveTo(Owner.DiscardPile);

            return true;
        }


        protected abstract List<EffectTemplate> CreateEffectList();
    }
}
