using System.Collections.Generic;

namespace Archetype.Game.Payloads.Pieces
{
    public class CardSet : ICardSet
    {
        private readonly List<ICard> _cards = new();

        public string Name { get; set; }
        public IEnumerable<ICard> Cards => _cards;

        public void AddCard(ICard card)
        {
            _cards.Add(card);
        }
    }
}