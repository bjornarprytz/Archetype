using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public abstract class Unit : GamePiece
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

        public bool IsAlive => Resources.Amount<Life>() > 0;

        public Deck Deck { get; set; }
        public Hand Hand { get; set; }
        public DiscardPile DiscardPile { get; set; }
        public List<EffectSpan> ActiveEffects { get; set; }
        public string Name { get; set; }

        public ResourcePool Resources { get; set; }
        public int Speed { get; set; } // Determines initiative order
        public int NextMoveTick { get; set; }

        public int HandLimit { get; set; }

        private Dictionary<string, int> _modifierAsSource;
        private Dictionary<string, int> _modifierAsTarget;

        public Unit(string name, ResourcePool resources, Faction team) : base(team)
        {
            _modifierAsSource = new Dictionary<string, int>();
            _modifierAsTarget = new Dictionary<string, int>();
            Name = name;

            Resources = resources;

            Deck = new Deck(this);
            Hand = new Hand(this);
            DiscardPile = new DiscardPile(this);
            ActiveEffects = new List<EffectSpan>();
        }
        
        public void AddModifierAsSource(Modifier modifier)
        {
            AddModifier(_modifierAsSource, modifier);
        }

        public void AddModifierAsTarget(Modifier modifier)
        {
            AddModifier(_modifierAsTarget, modifier);
        }

        public int ModifierAsSource(string keyword)
        {
            return GetModifier(_modifierAsSource, keyword);
        }
        public int ModifierAsTarget(string keyword)
        {
            return GetModifier(_modifierAsTarget, keyword);
        }

        public bool HasTurn(int tick)
        {
            return NextMoveTick == tick;
        }

        internal abstract void TakeTurn(Timeline timeline, GameState gameState, RequiredAction prompt);

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

        internal void Discard(int x, RequiredAction prompt)
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

            Decision result = prompt(new Choose<Card>(x, Hand));

            foreach (Card card in result.ChosenPieces)
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

        internal void Mill(int x)
        {
            while (x > 0)
            {
                Mill();
                x--;
            }
        }

        internal void Shuffle()
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


        private void Mill()
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

        private void AddModifier(Dictionary<string, int> modifierCollection, Modifier modifier)
        {
            string keyword = modifier.Keyword;

            if (!modifierCollection.ContainsKey(keyword)) modifierCollection[keyword] = 0;

            modifierCollection[keyword] += modifier.Value;
        }

        private int GetModifier(Dictionary<string, int> modifierCollection, string keyword)
        {
            return modifierCollection.ContainsKey(keyword) ? modifierCollection[keyword] : 0;
        }
    }
}