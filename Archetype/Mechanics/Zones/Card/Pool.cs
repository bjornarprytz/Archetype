using System.Collections.Generic;

namespace Archetype
{
    public class Pool : Zone<Card>, IOwned<Unit>
    {
        public Unit Owner { get; set; }
        public List<Card> Cards { get; set; }

        public Pool(Unit owner)
        {
            Owner = owner;
            Cards = new List<Card>();
        }

        public void Add(Card newCard) => Cards.Add(newCard);
        public void Remove(Card cardToRemove) => Cards.Remove(cardToRemove);
        public override IEnumerator<Card> GetEnumerator() => Cards.GetEnumerator();

        public void Populate(Deck deck)
        {
            foreach(var card in Cards)
            {
                var copy = card.MakeCopy(deck.Owner);
                copy.MoveTo(deck);
            }
        }
    }
}
