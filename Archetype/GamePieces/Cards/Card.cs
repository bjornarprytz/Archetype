
using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class Card : 
        GamePiece, 
        IOwned<Unit>, 
        IZoned<Card>, 
        ITarget,
        ITriggerAttachee<Card>,
        IModifierAttachee<Card>,
        IResponseAttachee<Card>,
        ICardFactory
    {
        public event EventHandler<ZoneChangeArgs<Card>> OnZoneChanged;

        public delegate void BeforePlay();
        public delegate void AfterPlay();

        public event BeforePlay OnBeforePlay;
        public event AfterPlay OnAfterPlay;

        public event EventHandler<ActionInfo> OnTargetOfActionBefore;
        public event EventHandler<ActionInfo> OnTargetOfActionAfter;

        public Unit Owner { get; set; }
        public CardData Data { get; private set; }

        public Zone<Card> CurrentZone { get; private set; }

        public TypeDictionary<ActionModifier<Card>> ActionModifiers { get; private set; }
        public List<Trigger<Card>> Triggers { get; private set; }
        public List<ActionResponse<Card>> Responses { get; private set; }

        internal Card(Unit owner, CardData data)
        {
            Owner = owner;
            Data = data;

            ActionModifiers = new TypeDictionary<ActionModifier<Card>>();
            Triggers = new List<Trigger<Card>>();
            Responses = new List<ActionResponse<Card>>();
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


        
        public virtual void PreActionAsTarget(ActionInfo action) 
        {
            OnTargetOfActionBefore?.Invoke(this, action);
        }
        public virtual void PostActionAsTarget(ActionInfo action) 
        {
            OnTargetOfActionAfter?.Invoke(this, action);
        }

        public void AttachModifier<TMod>(TMod modifier)
            where TMod : ActionModifier<Card>
        {
            if (ActionModifiers.Has<TMod>())
            {
                ActionModifiers.Get<TMod>().StackModifiers(modifier);
            }
            else
            {
                ActionModifiers.Set<TMod>(modifier);
                modifier.AttachHandler(this);
            }
        }

        public void DetachModifier<TMod>()
            where TMod : ActionModifier<Card>
        {
            if (ActionModifiers.Has<TMod>())
            {
                ActionModifiers.Get<TMod>().DetachHandler(this);
                ActionModifiers.Remove<TMod>();
            }
        }

        public void AttachTrigger(Trigger<Card> trigger)
        {
            Triggers.Add(trigger);
            trigger.AttachHandler(this);
        }
        public void DetachTrigger(Trigger<Card> trigger)
        {
            trigger.DetachHandler(this);
            Triggers.Remove(trigger);
        }

        public void AttachResponse(ActionResponse<Card> response)
        {
            response.AttachHandler(this);
            Responses.Add(response);
        }

        public void DetachResponse(ActionResponse<Card> response)
        {
            response.DetachHandler(this);
            Responses.Remove(response);
        }
    }
}
