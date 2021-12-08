
using Archetype.Game.Payloads.Proto;

namespace Archetype.Builder
{
    public class StructureBuilder : IBuilder<IStructureProtoData>
    {
        private readonly StructureProtoData _structureProtoData;
        
        internal StructureBuilder()
        {
            _structureProtoData = new StructureProtoData();
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
            _structureProtoData.MetaData = _structureProtoData.MetaData with { Name = name };

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
        

        public IStructureProtoData Build()
        {
            return _structureProtoData;
        }
    }
}