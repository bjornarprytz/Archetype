
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
        public event EventHandler<ZoneChangeEventArgs<Card>> OnZoneChanged;

        public event Action OnBeforePlay;
        public event Action OnAfterPlay;

        public event EventHandler<ActionInfo> OnTargetOfActionBefore;
        public event EventHandler<ActionInfo> OnTargetOfActionAfter;

        public Unit Owner { get; set; }
        public CardData Data { get; private set; }

        public Zone<Card> CurrentZone { get; private set; }

        public List<ActionModifier<Card>> Modifiers { get; private set; }
        public List<Trigger<Card>> Triggers { get; private set; }
        public List<ActionResponse<Card>> Responses { get; private set; }

        internal Card(Unit owner, CardData data)
        {
            Owner = owner;
            Data = data;

            Modifiers = new List<ActionModifier<Card>>();
            Triggers = new List<Trigger<Card>>();
            Responses = new List<ActionResponse<Card>>();
        }

        public void MoveTo(Zone<Card> newZone)
        {
            if (CurrentZone == newZone) return;

            CurrentZone?.Eject(this);

            CurrentZone = newZone;

            newZone?.Insert(this);

            OnZoneChanged?.Invoke(this, new ZoneChangeEventArgs<Card>(this, CurrentZone, newZone));
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
            gameState.ActionQueue.EnqueueActions(
                Data.Actions.Zip(args.TargetInfos, 
                (actionData, targets) => actionData.CreateAction(Owner, targets, gameState))
                .SelectMany(a => a));
        }

        public IList<ISelectionInfo<ITarget>> GetTargetRequirements(GameState gameState)
        {
            return Data.Actions.Select(a => a.TargetRequirements.GetSelectionInfo(Owner, gameState)).ToList();
        }

        public void PreActionAsTarget(ActionInfo action) 
        {
            OnTargetOfActionBefore?.Invoke(this, action);
        }
        public void PostActionAsTarget(ActionInfo action) 
        {
            OnTargetOfActionAfter?.Invoke(this, action);
        }
    }
}
