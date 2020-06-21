using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class Deck : Zone<Card>, IOwned<Unit>
    {
        public Unit Owner { get; set; }
        public Stack<Card> Cards { get; set; }
        public int Count => Cards.Count;
        public Deck(Unit owner, IEnumerable<Card> cards)
        {
            Owner = owner;

            Cards = new Stack<Card>();

            PutCardsOnTop(cards);
        }

        public Deck(Unit owner)
        {
            Owner = owner;
            Cards = new Stack<Card>();
        }

        public Card PeekTop()
        {
            if (Cards.Count == 0) return null; // TODO: No null

            return Cards.Peek();
        }

        public List<Card> LookAtTop(int nCards)
        {
            List<Card> cardsToLookAt = new List<Card>();

            if (Cards.Count < nCards) nCards = Cards.Count;


            foreach(Card card in Cards)
            {
                if (nCards == 0) break;

                cardsToLookAt.Add(card);

                nCards--;
            }

            return cardsToLookAt;
        }

        public void Draw(Zone<Card> into)
        {
            if (Cards.Count == 0) return; // TODO: No null
            Card card = Cards.Peek();
            card.MoveTo(into);
        }

        public void Shuffle()
        {
            Random rand = new Random();

            Card[] pile = Cards.ToArray();

            // Knuth-Fisher-Yates shuffle algorithm
            for (int i = pile.Length - 1; i > 0; i--)
            {
                int n = rand.Next(i + 1);
                Swap(ref pile[i], ref pile[n]);
            }

            Cards.Clear();
            
            // Rebuild the deck.
            foreach(Card card in pile)
            {
                Cards.Push(card);
            }
        }

        public void PutCardsOnTop(IEnumerable<Card> newCards)
        {
            foreach(Card card in newCards)
            {
                card.MoveTo(this);
            }
        }

        public void PutCardsOnBottom(IEnumerable<Card> newCards)
        {
            List<Card> newDeck = (List<Card>)newCards;

            Cards.Reverse();

            foreach(Card card in newDeck)
            {
                Cards.Push(card);
            }

            Cards.Reverse();

            // TODO: Fix this shitty function.
        }

        private void Swap(ref Card c1, ref Card c2)
        {
            Card temp = c1;
            c1 = c2;
            c2 = temp;
        }

        public override IEnumerator<Card> GetEnumerator() =>  ((IEnumerable<Card>)Cards).GetEnumerator();

        protected override void EjectInternal(Card cardToMove)
        {
            Stack<Card> cardsToKeep = new Stack<Card>();

            while (Cards.Count > 0)
            {
                Card candidate = Cards.Pop();

                if (candidate == cardToMove) break; // Found it, no need to keep looking

                cardsToKeep.Push(candidate);
            }

            while (cardsToKeep.Count > 0) // Rebuild the original stack
            {
                Cards.Push(cardsToKeep.Pop());
            }
        }

        protected override void InsertInternal(Card cardToMove)
        {
            Cards.Push(cardToMove);
        }
    }
}
