using System;
using System.Collections.Generic;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Pieces
{
    public interface ICardSet
    {
        string Name { get; set; }
        IEnumerable<ICardProtoData> Cards { get; }
        
        ICardProtoData this[Guid guid] { get; }
    }
    
    public class CardSet : ICardSet
    {
        private readonly Dictionary<Guid, ICardProtoData> _cards;

        public CardSet(Dictionary<Guid, ICardProtoData> cards)
        {
            _cards = cards;
        }

        public string Name { get; set; }
        public IEnumerable<ICardProtoData> Cards => _cards.Values;

        public ICardProtoData this[Guid guid] => _cards.ContainsKey(guid) ? _cards[guid] : default;
    }
}