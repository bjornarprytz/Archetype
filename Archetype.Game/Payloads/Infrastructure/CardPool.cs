using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Infrastructure
{
    public interface ICardPool
    {
        ICardProtoData this[Guid guid] { get; }
        IEnumerable<ICardProtoData> Cards { get; }
        IEnumerable<ICardSet> Sets { get; }
    }
    
    public class CardPool : ICardPool
    {
        private readonly List<ICardSet> _sets;
        
        public CardPool(List<ICardSet> sets)
        {
            _sets = sets;
        }
        
        public ICardProtoData this[Guid guid]=> _sets.Where(set => set[guid] != null).Select(set => set[guid]).FirstOrDefault();
        public IEnumerable<ICardProtoData> Cards => _sets.SelectMany(set => set.Cards);
        public IEnumerable<ICardSet> Sets => _sets;
    }
}