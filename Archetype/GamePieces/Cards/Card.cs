
using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class Card : GamePiece, IOwned<Unit>, IZoned<Card>, IHoldCounters, ITarget, ICardFactory
    {
        public event EventHandler<ZoneChangeArgs<Card>> OnZoneChanged;

        public delegate void BeforePlay();
        public delegate void AfterPlay();

        public event BeforePlay OnBeforePlay;
        public event AfterPlay OnAfterPlay;

        public Unit Owner { get; set; }
        public CardData Data { get; private set; }

        public TypeDictionary<Counter> ActiveCounters { get; private set; }

        public Zone<Card> CurrentZone { get; private set; }

        internal Card(Unit owner, CardData data)
        {
            Owner = owner;
            Data = data;
            ActiveCounters = new TypeDictionary<Counter>();
        }

        public void MoveTo(Zone<Card> newZone)
        {
            if (CurrentZone == newZone) return;

            CurrentZone?.Eject(this);

            CurrentZone = newZone;

            newZone?.Insert(this);

            OnZoneChanged?.Invoke(this, new ZoneChangeArgs<Card>(this, CurrentZone, newZone));
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
                    gameState.ActionQueue.EnqueueAction(action);
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
