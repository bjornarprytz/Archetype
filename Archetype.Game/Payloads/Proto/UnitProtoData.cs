using System;
using System.Collections.Generic;
using Archetype.Dto.MetaData;

namespace Archetype.Game.Payloads.Proto
{
    public interface IUnitProtoData
    {
        Guid Guid { get; }
        
        int Strength { get; }
        int Health { get; }
        int Defense { get; }
        UnitMetaData MetaData { get; }
        IEnumerable<ICardProtoData> Cards { get; }
    }
    
    public class UnitProtoData : IUnitProtoData
    {
        private readonly List<ICardProtoData> _cards;

        public UnitProtoData(List<ICardProtoData> cards)
        {
            Guid = Guid.NewGuid();
            _cards = cards;
        }

        public Guid Guid { get; }
        public int Strength { get; set; }
        public int Health { get; set; }
        public int Defense { get; set; }
        public UnitMetaData MetaData { get; set; }
        public IEnumerable<ICardProtoData> Cards => _cards;
    }
}