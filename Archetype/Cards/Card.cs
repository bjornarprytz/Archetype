
using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class Card : GamePiece, IOwned<Unit>, IZoned<Card>, IHoldCounters, ITarget, ICardFactory
    {
        public event ZoneChange<Card> OnZoneChanged;

        public delegate void BeforePlay();
        public delegate void AfterPlay();

        public event BeforePlay OnBeforePlay;
        public event AfterPlay OnAfterPlay;

        public Unit Owner { get; set; }
        public CardData Data { get; private set; }

        public TypeDictionary<Counter> ActiveCounters { get; private set; }

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

        internal Card(Unit owner, CardData data)
        {
            Owner = owner;
            Data = data;
            ActiveCounters = new TypeDictionary<Counter>();
        }

        public void MoveTo(Zone<Card> newZone)
        {
            // TODO: Move this implementation to the IZoned interface (available in C# 8.0)

            if (CurrentZone == newZone) return;

            if (CurrentZone != null) CurrentZone.Out(this);
            if (newZone != null) newZone.Into(this);

            CurrentZone = newZone;
        }

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

            PlayActual(args, gameState);

            OnAfterPlay?.Invoke();

            MoveTo(Owner.DiscardPile);

            return true;
        }

        public Card MakeCopy(Unit owner) => new Card(owner, Data);

        protected void PlayActual(PlayCardArgs args, GameState gameState)
        {
            if (!args.Valid) throw new Exception("WHat the hell, PlayCardArgs are invalid!?");

            foreach(var actions in Data.Actions.Zip(args.TargetInfos, (a, t) => a.CreateAction(Owner, t, gameState)))
            {
                foreach (var action in actions)
                {
                    gameState.ActionQueue.Enqueue(action);
                }
            }
        }

        public virtual void PostActionAsTarget(ActionInfo action) { }
        public virtual void PreActionAsTarget(ActionInfo action) { }

        public IList<ISelectionInfo<ITarget>> GetTargetRequirements(GameState gameState)
        {
            return Data.Actions.Select(a => a.TargetRequirements.GetSelectionInfo(Owner, gameState)).ToList();
        }
    }
}
