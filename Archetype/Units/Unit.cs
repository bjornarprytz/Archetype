using System;
using System.Collections.Generic;

namespace Archetype
{
    public abstract class Unit : GamePiece
    {
        public delegate void DamageTaken(Unit source, int damage);
        public delegate void DamageDealt(Unit target, int damage);
        public delegate void CardDrawn(Card drawnCard);
        public delegate void CardDiscarded(Card discardedCard);
        public delegate void CardMilled(Card milledCard);
        public delegate void DeckShuffled(Deck deck);

        public event DamageTaken OnDamageTaken;
        public event DamageDealt OnDamageDealt;
        public event CardDrawn OnCardDrawn;
        public event CardDiscarded OnCardDiscarded;
        public event CardMilled OnCardMilled;
        public event DeckShuffled OnDeckShuffled;

        public Deck Deck { get; set; }
        public Hand Hand { get; set; }
        public DiscardPile DiscardPile { get; set; }
        public List<EffectSpan> ActiveEffects { get; set; }
        public string Name { get; set; }
        public int Life { get; set; }
        public int Mana { get; set; }
        public int Speed { get; set; } // Determines initiative order
        public int NextMoveTick { get; set; }

        public int HandLimit { get; set; }

        public Dictionary<string, List<Modifier>> ModifiersAsSource { get; set; } // Keyword -> Modifier
        public Dictionary<string, List<Modifier>> ModifiersAsTarget { get; set; } // Keyword -> Modifier

        public Unit(string name, List<Card> cards) : base()
        {
            ModifiersAsSource = new Dictionary<string, List<Modifier>>();
            ModifiersAsTarget = new Dictionary<string, List<Modifier>>();
            Name = name;

            Deck = new Deck(this, cards);
            Hand = new Hand(this);
            DiscardPile = new DiscardPile(this);
            ActiveEffects = new List<EffectSpan>();
        }

        public List<Modifier> GetModifiersAsSource(string keyword) => ModifiersAsSource[keyword] ?? new List<Modifier>();
        public List<Modifier> GetModifiersAsTarget(string keyword) => ModifiersAsTarget[keyword] ?? new List<Modifier>();
        
        public void AddModifierAsSource(Modifier modifier)
        {
            AddModifier(ModifiersAsSource, modifier);
        }

        public void AddModifierAsTarget(Modifier modifier)
        {
            AddModifier(ModifiersAsTarget, modifier);
        }

        public bool HasTurn(int tick)
        {
            return NextMoveTick == tick;
        }

        public abstract void TakeTurn(GameState gameState);

        public void Discard(Guid cardId)
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

        public void Draw()
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

        public void Mill()
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

        public void Shuffle()
        {
            Deck.Shuffle();
            OnDeckShuffled?.Invoke(Deck);
        }

        internal int TakeDamage(DamageEffect effect)
        {
            int damageDone = effect.ModOutput(GetModifiersAsTarget(effect.Keyword));

            Life -= damageDone;
            OnDamageTaken?.Invoke(effect.Source, damageDone);

            return damageDone;
        }

        internal void DealDamage(DamageEffect effect)
        {
            effect.ModEffect(GetModifiersAsSource(effect.Keyword));

            foreach (Unit target in effect.Targets)
            {
                OnDamageDealt?.Invoke(target, target.TakeDamage(effect));
            }
        }

        internal void TakeCards(DrawCards effect)
        {
            int nCardsToDraw = effect.ModOutput(GetModifiersAsTarget(effect.Keyword));

            while (nCardsToDraw > 0)
            {
                Draw();
                nCardsToDraw--;
            }
        }

        internal void DealCards(DrawCards effect)
        {
            effect.ModEffect(GetModifiersAsSource(effect.Keyword));

            foreach (Unit target in effect.Targets)
            {
                target.TakeCards(effect);
            }
        }

        internal void TakeMill(MillCards effect)
        {
            int nCardsToMill = effect.ModOutput(GetModifiersAsTarget(effect.Keyword));

            while (nCardsToMill > 0)
            {
                Mill();
                nCardsToMill--;
            }
        }

        internal void DealMill(MillCards effect)
        {
            effect.ModEffect(GetModifiersAsSource(effect.Keyword));

            foreach (Unit target in effect.Targets)
            {
                target.TakeMill(effect);
            }
        }


        private void AddModifier(Dictionary<string, List<Modifier>> modifierCollection, Modifier modifier)
        {
            string keyword = modifier.Keyword;

            if (!modifierCollection.ContainsKey(keyword)) modifierCollection[keyword] = new List<Modifier>();

            modifierCollection[keyword].Add(modifier);
        }
    }
}