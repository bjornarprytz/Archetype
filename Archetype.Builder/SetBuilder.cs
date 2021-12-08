using System;
using System.Collections.Generic;
using Archetype.Builder.Exceptions;
using Archetype.Builder.Extensions;
using Archetype.Builder.Factory;
using Archetype.Game.Payloads.MetaData;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Builder
{
    public class SetBuilder : IBuilder<ISet>
    {

        private readonly ISet _setData;
        private readonly Dictionary<Guid, ICardProtoData> _cards = new();
        private readonly Dictionary<Guid, ICreatureProtoData> _creatures = new();
        private readonly Dictionary<Guid, IStructureProtoData> _structures = new();

        private CardMetaData _cardTemplate = new();
        private StructureMetaData _structureTemplate = new();
        private CreatureMetaData _creatureTemplate = new();

        internal SetBuilder(string name)
        {
            _cardTemplate = _cardTemplate with { SetName = name };
            
            _setData = new Set(_cards, _creatures, _structures)
            {
                Name = name
            };
        }

        public SetBuilder ChangeCardTemplate(Func<CardMetaData, CardMetaData> changeFunc)
        {
            _cardTemplate = changeFunc(_cardTemplate) with { SetName = _setData.Name };

            return this;
        }
        
        public SetBuilder ChangeCreatureTemplate(Func<CreatureMetaData, CreatureMetaData> changeFunc)
        {
            _creatureTemplate = changeFunc(_creatureTemplate) with { SetName = _setData.Name };

            return this;
        }
        
        public SetBuilder ChangeStructureTemplate(Func<StructureMetaData, StructureMetaData> changeFunc)
        {
            _structureTemplate = changeFunc(_structureTemplate) with { SetName = _setData.Name };

            return this;
        }

        public SetBuilder Card(Action<CardBuilder> builderProvider)
        {
            var cbc = BuilderFactory.CardBuilder(_cardTemplate);

            builderProvider(cbc);

            var card = cbc.Build();
            
            _cards.Add(card.Guid, card);

            return this;
        }

        public SetBuilder Creature(Action<CreatureBuilder> builderProvider)
        {
            var cbc = BuilderFactory.CreatureBuilder(_creatureTemplate);

            builderProvider(cbc);

            var creature = cbc.Build();
            
            _creatures.Add(creature.Guid, creature);

            return this;
        }
        
        public SetBuilder Structure(Action<StructureBuilder> builderProvider)
        {
            var cbc = BuilderFactory.StructureBuilder(_structureTemplate);

            builderProvider(cbc);

            var structure = cbc.Build();
            
            _structures.Add(structure.Guid, structure);

            return this;
        }

        public ISet Build()
        {
            if (_setData.Name.IsMissing())
                throw new MissingSetNameException();
            
            Console.WriteLine($"Created set {_setData.Name} with {_cards.Count} cards");

            return _setData;
        }
    }
}
