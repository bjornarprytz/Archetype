using System.Collections.Generic;
using Archetype.Core;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Pieces
{
    public class CardSet : ICardSet
    {
        private List<ICardProtoData> _cards = new ();

        public string Name { get; set; }
        public IEnumerable<ICardProtoData> Cards { get; }
        public void AddCard(ICardProtoData cardData)
        {
            _cards.Add(cardData);
        }
    }
}