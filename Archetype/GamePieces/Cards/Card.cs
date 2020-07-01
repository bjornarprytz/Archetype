
using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class Card : GamePiece, IOwned<Unit>, IZoned<Card>, ITarget, ICardFactory
    {
        public event EventHandler<ZoneChangeArgs<Card>> OnZoneChanged;

        public delegate void BeforePlay();
        public delegate void AfterPlay();

        public event BeforePlay OnBeforePlay;
        public event AfterPlay OnAfterPlay;

        public Unit Owner { get; set; }
        public CardData Data { get; private set; }

        public TypeDictionary<Counter> ActiveCounters { get; private set; }
        public TypeDictionary<Trigger> Triggers { get; private set; }

        public Zone<Card> CurrentZone { get; private set; }

        internal Card(Unit owner, CardData data)
        {
            Owner = owner;
            Data = data;
            ActiveCounters = new TypeDictionary<Counter>();
            Triggers = new TypeDictionary<Trigger>();
        }

        public void MoveTo(Zone<Card> newZone)
        {
            if (CurrentZone == newZone) return;

            CurrentZone?.Eject(this);

            CurrentZone = newZone;

            newZone?.Insert(this);

            OnZoneChanged?.Invoke(this, new ZoneChangeArgs<Card>(this, CurrentZone, newZone));
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


        public IList<ISelectionInfo<ITarget>> GetTargetRequirements(GameState gameState)
        {
            return Data.Actions.Select(a => a.TargetRequirements.GetSelectionInfo(Owner, gameState)).ToList();
        }

        public void AttachTrigger<T>(T trigger) where T : Trigger
        {
            if (Triggers.Has<T>())
            {
                Triggers.Get<T>().Stack(trigger);
            }
            else
            {
                trigger.AttachTo(this);
                Triggers.Set<T>(trigger);
            }
        }
        
        public virtual void PostActionAsTarget(ActionInfo action) { }
        public virtual void PreActionAsTarget(ActionInfo action) { }

    }
}
