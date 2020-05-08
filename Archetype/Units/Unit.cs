using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public abstract class Unit : GamePiece, IZoned<Unit>
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

        public bool IsAlive => Resources.Amount<Life>() > 0;

        public Deck Deck { get; set; }
        public Hand Hand { get; set; }
        public DiscardPile DiscardPile { get; set; }
        public string Name { get; set; }

        public bool HasMovesAvailable => Hand.Any(c => Resources.CanAfford(c.Cost));
        public ResourcePool Resources { get; set; }
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

        public Unit(string name, ResourcePool resources, Faction team) : base(team)
        {
            ModifiersAsSource = new EffectModifiers();
            ModifiersAsTarget = new EffectModifiers();
            Name = name;

            Resources = resources;

            Deck = new Deck(this);
            Hand = new Hand(this);
            DiscardPile = new DiscardPile(this);
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

        internal void Discard(Guid cardId)
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

        internal void Discard(int x, IPromptable prompt)
        {
            if (x < 1) return;

            if (Hand.IsEmpty)
            {
                Console.WriteLine($"\"I don't even have a single card in my hand... Can't discard shit\" says {Name}");
                return;
            }

            if (x >= Hand.Count)
            {
                foreach (Card card in Hand) Discard(card.Id);
                return;
            }

            Choose<Card> choose = new Choose<Card>(this, x, Hand);

            var response = prompt.PromptImmediate(choose);

            foreach (Card card in response.Choices)
            {
                Discard(card.Id);
            }
        }

        internal void Draw(int x)
        {
            while (x > 0)
            {
                Draw();
                x--;
            }
        }

        internal void ShuffleDeck()
        {
            Deck.Shuffle();
            OnDeckShuffled?.Invoke(Deck);
        }

        internal int ReceiveHeal(Unit source, int healAmount)
        {
            Resources.Gain(new Payment<Life>(healAmount));
            OnHealReceived?.Invoke(source, healAmount);

            return healAmount;
        }

        internal void GiveHeal(Unit target, int heal)
        {
            OnHealGiven(target, target.ReceiveHeal(this, heal));
        }

        internal int TakeDamage(Unit source, int damageTaken)
        {
            Resources.ForcePay(new Payment<Life>(damageTaken));
            OnDamageTaken?.Invoke(source, damageTaken);

            return damageTaken;
        }

        internal void DealDamage(Unit target, int damage)
        {
            OnDamageDealt?.Invoke(target, target.TakeDamage(this, damage));
        }

        internal int ResourceDifference<R>(Unit other) where R : Resource
        {
            return Resources.Amount<R>() - other.Resources.Amount<R>();
        }

        private void Draw()
        {
            Card cardToDraw = Deck.PeekTop();

            if (cardToDraw == null)
            {
                Console.WriteLine($"\"Fuck everything! My deck is empty!\" says {Name}");
                return;
            }

            cardToDraw.MoveTo(Hand);
            OnCardDrawn?.Invoke(cardToDraw);
        }
    }
}