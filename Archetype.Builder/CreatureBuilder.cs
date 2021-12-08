using Archetype.Game.Payloads.Proto;

namespace Archetype.Builder
{
    public class CreatureBuilder : IBuilder<ICreatureProtoData>
    {
        private readonly CreatureProtoData _creatureProtoData;
       
        internal CreatureBuilder()
        {
            _creatureProtoData = new CreatureProtoData();
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
            _creatureProtoData.MetaData = _creatureProtoData.MetaData with { Name = name };

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
        
        public ICreatureProtoData Build()
        {
            return _creatureProtoData;
        }
    }
}