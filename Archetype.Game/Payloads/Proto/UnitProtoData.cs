using System;
using System.Collections.Generic;
using Archetype.Dto.Composite;

namespace Archetype.Game.Payloads.Proto
{
    public interface IUnitProtoData
    {
        Guid Id { get; }
        
        int Health { get; }
        UnitMetaData MetaData { get; }
        IEnumerable<ICardProtoData> Cards { get; }
    }
    
    public class UnitProtoData : IUnitProtoData
    {
        private readonly List<ICardProtoData> _cards;

        public UnitProtoData(Guid id, UnitMetaData metaData, List<ICardProtoData> cards)
        {
            Id = id;
            MetaData = metaData;
            _cards = cards;
        }

        public Guid Id { get; }
        public int Health { get; }
        public UnitMetaData MetaData { get; }
        public IEnumerable<ICardProtoData> Cards => _cards;
    }
}