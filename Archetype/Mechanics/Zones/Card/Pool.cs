using System.Collections.Generic;

namespace Archetype
{
    public class Pool : Zone<Card>, IOwned<Unit>
    {
        public Unit Owner { get; set; }
        private List<Card> _cards { get; set; }

        public Pool(Unit owner)
        {
            Owner = owner;
            _cards = new List<Card>();
        }

        public void Add(Card newCard) => _cards.Add(newCard);
        public void Remove(Card cardToRemove) => _cards.Remove(cardToRemove);
        public override IEnumerator<Card> GetEnumerator() => _cards.GetEnumerator();

        public void Populate(Deck deck)
        {
            foreach(var card in _cards)
            {
                var copy = card.MakeCopy(deck.Owner);
                copy.MoveTo(deck);
            }
        }

        protected override void InsertInternal(Card pieceToMove)
        {
            _cards.Add(pieceToMove);
        }

        protected override void EjectInternal(Card pieceToEject)
        {
            _cards.Remove(pieceToEject);
        }
    }
}
