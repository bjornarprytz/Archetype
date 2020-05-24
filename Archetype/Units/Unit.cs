
using System;
using System.Linq;

namespace Archetype
{
    public abstract class Unit : GamePiece, IZoned<Unit>, IHoldCounters
    {

        public delegate void CardDrawn(Card drawnCard);
        public delegate void CardDiscarded(Card discardedCard);

        public delegate void CardMilled(Card milledCard);
        public delegate void DeckShuffled(Deck deck);

        public event CardDrawn OnCardDrawn;
        public event CardDiscarded OnCardDiscarded;
        public event CardMilled OnCardMilled;
        public event DeckShuffled OnDeckShuffled;

        public event EventHandler<ActionInfo> OnTargetOfActionBefore;
        public event EventHandler<ActionInfo> OnTargetOfActionAfter;
        public event EventHandler<ActionInfo> OnSourceOfActionBefore;
        public event EventHandler<ActionInfo> OnSourceOfActionAfter;
        
        public event ZoneChange<Unit> OnZoneChanged;
        public event Action OnDied; 

        public bool IsAlive => Life > 0;

        public Deck Deck { get; set; }
        public Hand Hand { get; set; }
        public DiscardPile DiscardPile { get; set; }
        public string Name { get; set; }

        public bool HasMovesAvailable => Hand.Any(c => Resources >= c.Cost);
        public int Resources { get; set; }
        public int Life 
        { 
            get => _life; 
            set 
            {
                bool wasAlive = IsAlive;

                _life = Math.Min(_life, MaxLife);

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
                OnZoneChanged?.Invoke(previousZone, currZone);
            }
        }
        private Zone<Unit> currZone;
        public TypeDictionary<Counter> ActiveCounters { get; private set; }


        public Unit(string name, int life, int resources, Faction team) : base(team)
        {
            Name = name;

            Life = life;
            Resources = resources;

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

        internal virtual void EndTurn() { }

        internal void Discard(int cardsToDiscard, IPromptable prompter)
        {
            if (cardsToDiscard < 1) return;

            if (Hand.IsEmpty)
            {
                Console.WriteLine($"\"I don't even have a single card in my hand... Can't discard shit\" says {Name}");
                return;
            }

            if (cardsToDiscard >= Hand.Count)
            {
                foreach (Card card in Hand) DiscardCard(card.Id);
                return;
            }

            Choose<Card> choose = new Choose<Card>(this, cardsToDiscard, Hand);

            var response = prompter.PromptImmediate(choose);

            foreach (Card card in response.Choices)
            {
                DiscardCard(card.Id);
            }
        }

        internal void DiscardCard(Guid cardId)
        {
            Card cardToDiscard = Hand.Pick(cardId);

            if (cardToDiscard == null)
            {
                Console.WriteLine($"\"He he! I don't even have that card!\" says {Name}");
                return;
            }

            cardToDiscard.MoveTo(DiscardPile);
            OnCardDiscarded?.Invoke(cardToDiscard);
        }

        internal void ShuffleDeck()
        {
            Deck.Shuffle();
            OnDeckShuffled?.Invoke(Deck);
        }

        public void Act(ActionInfo actionInfo, Action payload)
        {
            OnSourceOfActionBefore?.Invoke(this, actionInfo);

            actionInfo.Target.React(actionInfo, payload);

            OnSourceOfActionAfter?.Invoke(this, actionInfo);
        }

        public void React(ActionInfo action, Action payload)
        {
            OnTargetOfActionBefore?.Invoke(this, action);

            payload();

            OnTargetOfActionAfter?.Invoke(this, action);
        }

        internal void Heal(int amount) => Life += amount;

        internal void Damage(int amount) => Life -= amount;

        internal void Draw(int cardsToDraw)
        {
            for (int i=0; i < cardsToDraw; i++)
            {
                DrawCard();
            }
        }

        internal void Mill(int cardsToMill)
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
            OnCardDrawn?.Invoke(cardToDraw);
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
            OnCardMilled?.Invoke(cardToMill);
        }
    }
}