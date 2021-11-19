using System;
using System.Collections.Generic;
using Archetype.Builder.Factory;
using Archetype.Dto.MetaData;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Builder
{
    public class UnitBuilder : IBuilder<IUnitProtoData>
    {
        private readonly UnitProtoData _unitProtoData;

        private readonly List<ICardProtoData> _cards = new();
        
        internal UnitBuilder()
        {
            _unitProtoData = new UnitProtoData(_cards);
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