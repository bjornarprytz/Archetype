using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public class Deck : Zone, IEnumerable<Card>
    {
        public Stack<Card> Cards { get; set; }
        public int Count => Cards.Count;
        public Deck(Unit owner, IEnumerable<Card> cards) : base(owner)
        {
            Cards = new Stack<Card>();

            PutCardsOnTop(cards);
        }

        public Deck(Unit owner) : base(owner)
        {
            Cards = new Stack<Card>();
        }
        public static List<Card> Sample(int size)
        {
            List<Card> sampleList = new List<Card>();

            for (int i=0; i<size; i++)
            {
                Card newCard = new Card($"Card {i}");

                sampleList.Add(newCard);
            }

            return sampleList; 
        }
        public static List<Card> TestMoveSet(int size)
        {
            List<Card> sampleList = new List<Card>();

            for (int i = 0; i < size; i++)
            {
                Card newMove = new Card($"Card {i}");

                sampleList.Add(newMove);
            }

            return sampleList;
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

        public void Draw(Zone into)
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

        public IEnumerator<Card> GetEnumerator()
        {
            return ((IEnumerable<Card>)Cards).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Card>)Cards).GetEnumerator();
        }

        public override void Out(Card cardToMove)
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

            base.Out(cardToMove);
        }

        public override void Into(Card cardToMove)
        {
            Cards.Push(cardToMove);

            base.Into(cardToMove);
        }
    }
}
