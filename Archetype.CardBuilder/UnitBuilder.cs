using System;
using System.Collections.Generic;
using Archetype.CardBuilder.Factory;
using Archetype.Core.Data.Composite;
using Archetype.Game.Payloads.Proto;

namespace Archetype.CardBuilder
{
    public class UnitBuilder : IBuilder<IUnitProtoData>
    {
        private readonly UnitProtoData _unitProtoData;

        private readonly UnitMetaData _metaData = new();
        private readonly List<ICardProtoData> _cards = new();
        
        public UnitBuilder()
        {
            _unitProtoData = new UnitProtoData(Guid.NewGuid(), _metaData, _cards);
        }
        
        public UnitBuilder Card <TResult>(Action<CardBuilder> builderProvider)
        {
            var cbc = BuilderFactory.CardBuilder(); // Input template data here

            builderProvider(cbc);
            
            _cards.Add(cbc.Build());

            return this;
        }
        
        
        public IUnitProtoData Build()
        {
            return _unitProtoData;
        }
    }
}