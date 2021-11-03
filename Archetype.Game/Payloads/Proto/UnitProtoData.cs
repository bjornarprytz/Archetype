using System;
using System.Collections.Generic;
using Archetype.Game.Payloads.Metadata;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Core.Enemy
{
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
        public int Health { get; set; }
        public UnitMetaData MetaData { get; }
        public IEnumerable<ICardProtoData> Cards => _cards;
    }
}