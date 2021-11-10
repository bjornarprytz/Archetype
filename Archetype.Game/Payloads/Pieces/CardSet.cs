using System.Collections.Generic;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Pieces
{
    public interface ICardSet
    {
        string Name { get; set; }
        IEnumerable<ICardProtoData> Cards { get; }
    }
    
    public class CardSet : ICardSet
    {
        private readonly List<ICardProtoData> _cards;

        public CardSet(List<ICardProtoData> cards)
        {
            _cards = cards;
        }

        public string Name { get; set; }
        public IEnumerable<ICardProtoData> Cards => _cards;
    }
}