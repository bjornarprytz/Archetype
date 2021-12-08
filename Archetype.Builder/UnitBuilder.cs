using System;
using System.Collections.Generic;
using Archetype.Builder.Factory;
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
        
        public UnitBuilder Card<TResult>(Action<CardBuilder> builderProvider)
        {
            var cbc = BuilderFactory.CardBuilder(); // Input template data here

            builderProvider(cbc);
            
            _cards.Add(cbc.Build());

            return this;
        }

        public UnitBuilder Movement(int movement)
        {
            _unitProtoData.Movement = movement;
            
            return this;
        }
        
        public UnitBuilder Strength(int strength)
        {
            _unitProtoData.Strength = strength;
            
            return this;
        }

        public UnitBuilder Health(int health)
        {
            _unitProtoData.Health = health;

            return this;
        }

        public UnitBuilder Defense(int defense)
        {
            _unitProtoData.Defense = defense;

            return this;
        }
        
        public UnitBuilder Name(string name)
        {
            _unitProtoData.MetaData = _unitProtoData.MetaData with { Name = name };

            return this;
        }

        public UnitBuilder Art(string uri)
        {
            _unitProtoData.MetaData = _unitProtoData.MetaData with { ImageUri = uri };

            return this;
        }

        public UnitBuilder Level(int level)
        {
            _unitProtoData.MetaData = _unitProtoData.MetaData with { Level = level };

            return this;
        }
        
        public IUnitProtoData Build()
        {
            return _unitProtoData;
        }
    }
}