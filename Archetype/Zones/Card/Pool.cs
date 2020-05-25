using System.Collections.Generic;

namespace Archetype
{
    public class Pool<T> : Zone<Card>, IOwned<T> where T : GamePiece
    {
        public T Owner { get; set; }
        public List<Card> Cards { get; set; }

        public Pool(T owner, IEnumerable<Card> cards=null)
        {
            Owner = owner;
            Cards = new List<Card>(cards);
        }

        public void Add(Card newCard) => Cards.Add(newCard);
        public void Remove(Card cardToRemove) => Cards.Remove(cardToRemove);
        public override IEnumerator<Card> GetEnumerator() => Cards.GetEnumerator();
    }
}
