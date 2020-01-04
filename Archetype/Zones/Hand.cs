using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class Hand : Zone<Card>, IOwned<Unit>
    {
        public Unit Owner { get; set; }

        public bool IsEmpty => Cards.Count == 0;
        public int Count => Cards.Count;
        public Dictionary<Guid, Card> Cards { get; set; }
        public Hand(Unit owner)
        {
            Owner = owner;

            Cards = new Dictionary<Guid, Card>();
        }

        public Card Pick(Guid cardId)
        {
            if (!Cards.ContainsKey(cardId)) return null;

            return Cards[cardId];
        }

        public override IEnumerator<Card> GetEnumerator() => Cards.Values.GetEnumerator(); 

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
