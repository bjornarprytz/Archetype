using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class DiscardPile : Zone<Card>, IOwned<Unit>
    {
        public Unit Owner { get; set; }
        public int Count => Cards.Count;
        public Dictionary<Guid, Card> Cards { get; set; }
        public DiscardPile(Unit owner)
        {
            Owner = owner;

            Cards = new Dictionary<Guid, Card>();
        }

        public Card FindCard(Guid Id)
        {
            if (!Cards.ContainsKey(Id)) return null;

            return Cards[Id];
        }

        public override IEnumerator<Card> GetEnumerator() => Cards.Values.GetEnumerator();

        protected override void EjectInternal(Card cardToMove)
        {
            if (!Cards.ContainsKey(cardToMove.Id)) return;

            Cards.Remove(cardToMove.Id);
        }

        protected override void InsertInternal(Card cardToMove)
        {
            if (Cards.ContainsKey(cardToMove.Id)) return;

            Cards.Add(cardToMove.Id, cardToMove);
        }
    }
}
