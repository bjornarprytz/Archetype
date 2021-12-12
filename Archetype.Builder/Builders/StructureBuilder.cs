
using System.Collections.Generic;
using Archetype.Builder.Base;
using Archetype.Game.Payloads.Context.Effect;
using Archetype.Game.Payloads.Context.Trigger;
using Archetype.Game.Payloads.MetaData;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Builder
{
    public class StructureBuilder : ProtoBuilder<IStructureProtoData>
    {
        private readonly List<IEffect<ITriggerContext<IStructure>>> _effects = new();

        private readonly StructureProtoData _structureProtoData;
        
        internal StructureBuilder(StructureMetaData template)
        {
            _structureProtoData = new StructureProtoData(_effects)
            {
                MetaData = template
            };
        }

        
        public StructureBuilder Health(int health)
        {
            _structureProtoData.Health = health;

            return this;
        }

        public StructureBuilder Defense(int defense)
        {
            _structureProtoData.Defense = defense;

            return this;
        }
        
        public StructureBuilder Name(string name)
        {
            _structureProtoData.Name = name;

            return this;
        }

        public StructureBuilder Art(string uri)
        {
            _structureProtoData.MetaData = _structureProtoData.MetaData with { ImageUri = uri };

            return this;
        }

        public StructureBuilder Level(int level)
        {
            _structureProtoData.MetaData = _structureProtoData.MetaData with { Level = level };

            return this;
        }

        protected override IStructureProtoData BuildInternal()
        {
            return _structureProtoData;
        }
    }
}