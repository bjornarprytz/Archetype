
using System;
using System.Linq;

namespace Archetype
{
    public abstract class Unit : GamePiece, IZoned<Unit>, IOwned<Player>, IHoldCounters, ITarget, ISource
    {
        public event EventHandler<CardTriggerArgs> OnCardDrawn;
        public event EventHandler<CardTriggerArgs> OnCardDiscarded;
        public event EventHandler<CardTriggerArgs> OnCardMilled;
        public event EventHandler<GenericTriggerArgs<Deck>> OnDeckShuffled;

        public event EventHandler<GenericTriggerArgs<int>> OnDamaged;
        public event EventHandler<GenericTriggerArgs<int>> OnHealed;

        public event EventHandler<ActionInfo> OnTargetOfActionBefore;
        public event EventHandler<ActionInfo> OnTargetOfActionAfter;
        public event EventHandler<ActionInfo> OnSourceOfActionBefore;
        public event EventHandler<ActionInfo> OnSourceOfActionAfter;

        public event EventHandler<ZoneChangeTriggerArgs<Unit>> OnZoneChanged;
        public event Action OnDied;

        public bool IsAlive => Life > 0;

        public Pool CardPool { get; set; }
        public Deck Deck { get; set; }
        public Hand Hand { get; set; }
        public DiscardPile DiscardPile { get; set; }
        public string Name { get; set; }

        public bool HasMovesAvailable => Hand.Any(c => Resources >= c.Data.Cost);
        public int Resources { get; set; }
        public int Life
        {
            get => _life;
            set
            {
                bool wasAlive = IsAlive;

                _life = Math.Min(value, MaxLife);

                if (!IsAlive && wasAlive) OnDied?.Invoke();
            }
        }
        private int _life;
        public int MaxLife { get; set; }
        public int Speed { get; set; } // Determines initiative order

        public int HandLimit { get; set; }

        public Zone<Unit> CurrentZone
        {
            get { return currZone; }
            private set
            {
                Zone<Unit> previousZone = currZone;
                currZone = value;
                OnZoneChanged?.Invoke(this, new ZoneChangeTriggerArgs<Unit>(this, previousZone, currZone));
            }
        }
        private Zone<Unit> currZone;
        public TypeDictionary<Counter> ActiveCounters { get; private set; }

        public Player Owner { get; private set; }

        public Unit(Player owner, string name, int life, int resources) : base(owner.Team)
        {
            Name = name;
            Owner = owner;

            MaxLife = life;
            Life = life;
            Resources = resources;

            CardPool = new Pool(this);
            Deck = new Deck(this);
            Hand = new Hand(this);
            DiscardPile = new DiscardPile(this);
        }

        public void Apply<T>(T counter) where T : Counter
        {
            if (!ActiveCounters.Has<T>())
                ActiveCounters.Set<T>(counter);
            else
                ActiveCounters.Get<T>().Combine(counter);
        }

        public void Remove<T>() where T : Counter
        {
            ActiveCounters.Remove<T>();
        }

        public void MoveTo(Zone<Unit> newZone)
        {
            // TODO: Move this implementation to the IZoned interface (available in C# 8.0)

            if (CurrentZone == newZone) return;

            if (CurrentZone != null) CurrentZone.Out(this);
            if (newZone != null) newZone.Into(this);

            CurrentZone = newZone;
        }

        public virtual void EndTurn() { }

        public void Discard(int nCardsToDiscard, IPromptable prompter)
        {
            var choiceArgs = new Choose<Card>(nCardsToDiscard, Hand);

            prompter.Choose(choiceArgs);

            foreach (Card card in choiceArgs.Choices)
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
            OnCardDiscarded?.Invoke(this, new CardTriggerArgs(cardToDiscard));
        }

        public void ShuffleDeck()
        {
            Deck.Shuffle();
            OnDeckShuffled?.Invoke(this, new GenericTriggerArgs<Deck>(Deck));
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
            OnHealed?.Invoke(this, new GenericTriggerArgs<int>(amount));
        }

        public void Damage(int amount)
        {
            Life -= amount;
            OnDamaged?.Invoke(this, new GenericTriggerArgs<int>(amount));
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
            OnCardDrawn?.Invoke(this, new CardTriggerArgs(cardToDraw));
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
            OnCardMilled?.Invoke(this, new CardTriggerArgs(cardToMill));
        }
    }
}