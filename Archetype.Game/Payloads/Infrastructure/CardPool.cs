using System;
using System.Collections.Generic;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Infrastructure
{
    public interface ICardPool
    {
        ICardProtoData this[Guid guid] { get; }
        IEnumerable<ICardProtoData> Cards { get; }
    }
    
    public class CardPool : ICardPool
    {
        private readonly Dictionary<Guid, ICardProtoData> _cards = new();

        public CardPool(ICardSet set) // TODO: Remove this func asap
        {
            foreach (var card in set.Cards)
            {
                AddCard(card);
            }
        }
        
        public ICardProtoData this[Guid guid] => !_cards.ContainsKey(guid) ? default : _cards[guid];

        public IEnumerable<ICardProtoData> Cards => _cards.Values;

        public void AddCard(ICardProtoData card)
        {
            _cards.Add(card.Guid, card);
        }
    }
}