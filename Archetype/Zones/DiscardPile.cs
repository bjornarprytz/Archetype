using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class DiscardPile : Zone, IEnumerable<Card>
    {
        public int Count => Cards.Count;
        public Dictionary<Guid, Card> Cards { get; set; }
        public DiscardPile(Unit owner) : base(owner)
        {
            Cards = new Dictionary<Guid, Card>();
        }

        public Card FindCard(Guid Id)
        {
            if (!Cards.ContainsKey(Id)) return null;

            return Cards[Id];
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
            if (!Cards.ContainsKey(cardToMove.Id)) return;

            Cards.Remove(cardToMove.Id);

            base.Out(cardToMove);
        }

        public override void Into(Card cardToMove)
        {
            if (Cards.ContainsKey(cardToMove.Id)) return;

            Cards.Add(cardToMove.Id, cardToMove);

            base.Into(cardToMove);
        }
    }
}
