
using System;
using System.Linq;

namespace Archetype
{
    public abstract class Unit : GamePiece, IZoned<Unit>, IOwned<Player>, IHoldCounters, ITarget, ISource
    {
        public event EventHandler<ZoneChangeArgs<Unit>> OnZoneChanged;

        public event EventHandler<TriggerArgs> OnCardDrawn;
        public event EventHandler<TriggerArgs> OnCardDiscarded;
        public event EventHandler<TriggerArgs> OnCardMilled;
        public event EventHandler<TriggerArgs> OnDeckShuffled;

        public event EventHandler<TriggerArgs> OnDamaged;
        public event EventHandler<TriggerArgs> OnHealed;

        public event EventHandler<ActionInfo> OnTargetOfActionBefore;
        public event EventHandler<ActionInfo> OnTargetOfActionAfter;
        public event EventHandler<ActionInfo> OnSourceOfActionBefore;
        public event EventHandler<ActionInfo> OnSourceOfActionAfter;

        public event Action OnDied;

        public bool IsAlive => Life > 0;

        public IPrompter Prompter { get; private set; }
        public UnitData Data { get; private set; }

        public Pool CardPool { get; set; }
        public Deck Deck { get; set; }
        public Hand Hand { get; set; }
        public DiscardPile DiscardPile { get; set; }
        public string Name => Data.Name;

        public bool HasMovesAvailable => Hand.Any(c => Resources >= c.Data.Cost);
        public int Resources { get; set; }
        public int Life
        {
            get => _life;
            set
            {
                bool wasAlive = IsAlive;

                _life = Math.Min(value, Data.MaxLife);

                if (!IsAlive && wasAlive) OnDied?.Invoke();
            }
        }
        private int _life;
        public int MaxLife { get; set; }

        public Zone<Unit> CurrentZone { get; private set; }
        public TypeDictionary<Counter> ActiveCounters { get; private set; }
        public TypeDictionary<Trigger> Triggers { get; private set; }

        public Player Owner { get; private set; }


        public Unit(Player owner, UnitData data, IPrompter prompter) : base(owner.Team)
        {
            Prompter = prompter ?? throw new ArgumentException("Please provide an valid prompter");
            
            Data = data;
            Owner = owner;
            Life = data.MaxLife;
            Resources = data.StartingResources;

            CardPool = new Pool(this);
            Deck = new Deck(this);
            Hand = new Hand(this);
            DiscardPile = new DiscardPile(this);

            ActiveCounters = new TypeDictionary<Counter>();
            Triggers = new TypeDictionary<Trigger>();
        }

        public void MoveTo(Zone<Unit> newZone)
        {
            if (newZone == CurrentZone) return;

            CurrentZone?.Eject(this);
            
            CurrentZone = newZone;

            newZone?.Insert(this);

            OnZoneChanged?.Invoke(this, new ZoneChangeArgs<Unit>(this, CurrentZone, newZone));
        }

        public virtual void EndTurn() { }

        public void Discard(int nCardsToDiscard)
        {
            var choiceArgs = new ForcedSelectionInfo<Card>(nCardsToDiscard, Hand);

            Prompter.Choose(choiceArgs);

            foreach (Card card in choiceArgs.ConfirmedSelection)
            {
                DiscardCard(card.Id);
            }
        }

        public void DiscardCard(Guid cardId)
        {
            Card cardToDiscard = Hand.Pick(cardId);

            if (cardToDiscard == null)
            {
                Console.WriteLine($"\"He he! I don't even have that card!\" says {Name}");
                return;
            }

            cardToDiscard.MoveTo(DiscardPile);
            OnCardDiscarded?.Invoke(this, new TriggerArgs());
        }

        public void ShuffleDeck()
        {
            Deck.Shuffle();
            OnDeckShuffled?.Invoke(this, new TriggerArgs());
        }
        public void PreActionAsSource(ActionInfo action)
        {
            OnSourceOfActionBefore?.Invoke(this, action);
        }

        public void PostActionAsSource(ActionInfo action)
        {
            OnSourceOfActionAfter?.Invoke(this, action);
        }

        public void PreActionAsTarget(ActionInfo action)
        {
            OnTargetOfActionBefore?.Invoke(this, action);
        }

        public void PostActionAsTarget(ActionInfo action)
        {
            OnTargetOfActionAfter?.Invoke(this, action);
        }

        public void Heal(int amount)
        {
            Life += amount;
            OnHealed?.Invoke(this, new TriggerArgs());
        }

        public void Damage(int amount)
        {
            Life -= amount;
            OnDamaged?.Invoke(this, new TriggerArgs());
        }

        public void Draw(int cardsToDraw)
        {
            for (int i=0; i < cardsToDraw; i++)
            {
                DrawCard();
            }
        }

        public void Mill(int cardsToMill)
        {
            for (int i = 0; i < cardsToMill; i++)
            {
                MillCard();
            }
        }

        private void DrawCard()
        {
            Card cardToDraw = Deck.PeekTop();

            if (cardToDraw == null)
            {
                Console.WriteLine($"\"Can't draw! My deck is empty!\" says {Name}");
                return;
            }

            cardToDraw.MoveTo(Hand);
            OnCardDrawn?.Invoke(this, new TriggerArgs());
        }

        private void MillCard()
        {
            Card cardToMill = Deck.PeekTop();

            if (cardToMill == null)
            {
                Console.WriteLine($"\"Can't mill! My deck is empty!\" says {Name}");
                return;
            }

            cardToMill.MoveTo(DiscardPile);
            OnCardMilled?.Invoke(this, new TriggerArgs());
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
    }
}