using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class Hand : Zone, IEnumerable<Card>
    {
        public bool IsEmpty => Cards.Count == 0;
        public int Count => Cards.Count;
        public Dictionary<Guid, Card> Cards { get; set; }
        public Hand(Unit owner) : base(owner)
        {
            Cards = new Dictionary<Guid, Card>();
        }

        public Card Pick(Guid cardId)
        {
            if (!Cards.ContainsKey(cardId)) return null;

            return Cards[cardId];
        }

        public IEnumerator<Card> GetEnumerator() { return Cards.Values.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return Cards.GetEnumerator(); }

        public override void Out(Card cardToMove)
        {
            Cards.Remove(cardToMove.Id);

            base.Out(cardToMove);
        }

        public override void Into(Card cardToMove)
        {
            Cards.Add(cardToMove.Id, cardToMove);

            base.Into(cardToMove);
        }
    }
}
