using System.Collections.Generic;
using System.Linq;
using Archetype.Builder.Builders.Base;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Context.Effect.Base;
using Archetype.Game.Payloads.Proto;
using Archetype.View.Atoms.MetaData;

namespace Archetype.Builder.Builders
{
    public interface IStructureBuilder : IBuilder<IStructureProtoData>
    {
        IStructureBuilder MetaData(StructureMetaData metaData);
        IStructureBuilder Name(string name);
        IStructureBuilder Art(string link);
        IStructureBuilder Level(int level);
        
        IStructureBuilder Health(int health);
        IStructureBuilder Defense(int defense);
    }
    
    internal class StructureBuilder : ProtoBuilder<IStructureProtoData>, IStructureBuilder
    {
        private readonly List<IEffect> _effects = new();

        private readonly StructureProtoData _structureProtoData;
        
        public StructureBuilder()
        {
            _structureProtoData = new StructureProtoData(_effects);
        }
        
        public IStructureBuilder MetaData(StructureMetaData metaData)
        {
            _structureProtoData.MetaData = metaData;

            return this;
        }
        
        public IStructureBuilder Health(int health)
        {
            _structureProtoData.Health = health;

            return this;
        }

        public IStructureBuilder Defense(int defense)
        {
            _structureProtoData.Defense = defense;

            return this;
        }

        public IStructureBuilder Name(string name)
        {
            _structureProtoData.Name = name;

            return this;
        }

        public IStructureBuilder Art(string uri)
        {
            _structureProtoData.MetaData = _structureProtoData.MetaData with { ImageUri = uri };

            return this;
        }

        public IStructureBuilder Level(int level)
        {
            _structureProtoData.MetaData = _structureProtoData.MetaData with { Level = level };

            return this;
        }

        protected override IStructureProtoData BuildInternal()
        {
            _structureProtoData.EffectDescriptors = _effects.Select(effect => effect.CreateDescription()).ToList();
            
            return _structureProtoData;
        }
    }
}