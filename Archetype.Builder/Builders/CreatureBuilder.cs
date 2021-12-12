using Archetype.Builder.Builders.Base;
using Archetype.Game.Payloads.MetaData;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Builder.Builders
{
    public class CreatureBuilder : ProtoBuilder<ICreatureProtoData>
    {
        private readonly CreatureProtoData _creatureProtoData;
       
        internal CreatureBuilder(CreatureMetaData template)
        {
            _creatureProtoData = new CreatureProtoData()
            {
                MetaData = template
            };
        }

        public CreatureBuilder Movement(int movement)
        {
            _creatureProtoData.Movement = movement;
            
            return this;
        }
        
        public CreatureBuilder Strength(int strength)
        {
            _creatureProtoData.Strength = strength;
            
            return this;
        }

        public CreatureBuilder Health(int health)
        {
            _creatureProtoData.Health = health;

            return this;
        }

        public CreatureBuilder Defense(int defense)
        {
            _creatureProtoData.Defense = defense;

            return this;
        }
        
        public CreatureBuilder Name(string name)
        {
            _creatureProtoData.Name = name;

            return this;
        }

        public CreatureBuilder Art(string uri)
        {
            _creatureProtoData.MetaData = _creatureProtoData.MetaData with { ImageUri = uri };

            return this;
        }

        public CreatureBuilder Level(int level)
        {
            _creatureProtoData.MetaData = _creatureProtoData.MetaData with { Level = level };

            return this;
        }

        protected override ICreatureProtoData BuildInternal()
        {

            return _creatureProtoData;
        }
    }
}