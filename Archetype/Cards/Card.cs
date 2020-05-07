using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public class Card : GamePiece, IOwned<Unit>
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
        private List<EffectTemplate> _effectTemplates;
        
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

        internal Card(string name, CompoundPayment cost, List<EffectTemplate> effects=null)
        {
            Name = name;
            Cost = cost;
            _effectTemplates = effects ?? new List<EffectTemplate>();
        }

        internal void MoveTo(Zone<Card> newZone)
        {
            if (CurrentZone == newZone) return;

            if (CurrentZone != null) CurrentZone.Out(this);
            if (newZone != null) newZone.Into(this);

            CurrentZone = newZone;
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

        public IEnumerable<EffectArgs> GetArgs(GameState gameState) => _effectTemplates.Select(e => e.Args(Owner, gameState));
    }
}
