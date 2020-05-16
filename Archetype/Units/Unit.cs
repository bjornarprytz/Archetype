using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public abstract class Unit : GamePiece, IZoned<Unit>, IHoldCounters
    {
        public delegate void DamageTaken(Unit source, int damage);
        public delegate void DamageDealt(Unit target, int damage);
        public delegate void HealReceived(Unit source, int heal);
        public delegate void HealGiven(Unit target, int heal);
        public delegate void CardDrawn(Card drawnCard);
        public delegate void CardDiscarded(Card discardedCard);

        public delegate void CardMilled(Card milledCard);
        public delegate void DeckShuffled(Deck deck);

        public event DamageTaken OnDamageTaken;
        public event DamageDealt OnDamageDealt;
        public event HealReceived OnHealReceived;
        public event HealGiven OnHealGiven;
        public event CardDrawn OnCardDrawn;
        public event CardDiscarded OnCardDiscarded;
        public event CardMilled OnCardMilled;
        public event DeckShuffled OnDeckShuffled;
        
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

        private EffectModifiers ModifiersAsSource { get; set; }
        private EffectModifiers ModifiersAsTarget { get; set; }
        private TypeDictionary<Counter> ActiveCounters { get; set; }

        public Unit(string name, int life, int resources, Faction team) : base(team)
        {
            ModifiersAsSource = new EffectModifiers();
            ModifiersAsTarget = new EffectModifiers();
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

        internal void ModifyOutgoingEffect<T>(T xEffect) where T : XEffect
        {
            xEffect.X += ModifiersAsSource.Get<T>();
        }

        internal int ModifiedIncomingEffect<T>(T xEffect) where T : XEffect
        {
            return xEffect.X + ModifiersAsTarget.Get<T>();
        }

        internal virtual void EndTurn() { }

        internal void Discard(DiscardEffect discardEffect, IPromptable prompt)
        {
            int cardsToDiscard = ModifiedIncomingEffect(discardEffect);

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

            var response = prompt.PromptImmediate(choose);

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

        internal void Heal(HealEffect healEffect)
        {
            int amountToHeal = ModifiedIncomingEffect(healEffect);

            Life += amountToHeal;

            OnHealReceived?.Invoke(healEffect.Source, amountToHeal);
        }

        internal void Damage(DamageEffect damageEffect)
        {
            int damageToTake = ModifiedIncomingEffect(damageEffect);

            Life -= damageToTake;

            OnDamageTaken?.Invoke(damageEffect.Source, damageToTake);
        }

        internal void Draw(DrawEffect drawEffect)
        {
            int cardsToDraw = ModifiedIncomingEffect(drawEffect);

            for (int i=0; i < cardsToDraw; i++)
            {
                DrawCard();
            }
        }

        internal void Mill(MillEffect millEffect)
        {
            int cardsToMill = ModifiedIncomingEffect(millEffect);

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