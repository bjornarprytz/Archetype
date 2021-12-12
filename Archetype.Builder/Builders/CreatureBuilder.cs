using Archetype.Builder.Builders.Base;
using Archetype.Game.Payloads.MetaData;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Builder.Builders
{
    public interface ICreatureBuilder : IBuilder<ICreatureProtoData>
    {
        ICreatureBuilder MetaData(CreatureMetaData metaData);
        ICreatureBuilder Movement(int movement);
        ICreatureBuilder Strength(int strength);
        ICreatureBuilder Health(int health);
        ICreatureBuilder Defense(int defense);
        ICreatureBuilder Name(string name);
        ICreatureBuilder Art(string uri);
        ICreatureBuilder Level(int level);
    }

    public class CreatureBuilder : ProtoBuilder<ICreatureProtoData>, ICreatureBuilder
    {
        private readonly CreatureProtoData _creatureProtoData;

        public CreatureBuilder()
        {
            _creatureProtoData = new CreatureProtoData();
        }

        public ICreatureBuilder MetaData(CreatureMetaData metaData)
        {
            _creatureProtoData.MetaData = metaData;

            return this;
        }

        public ICreatureBuilder Movement(int movement)
        {
            _creatureProtoData.Movement = movement;

            return this;
        }

        public ICreatureBuilder Strength(int strength)
        {
            _creatureProtoData.Strength = strength;

            return this;
        }

        public ICreatureBuilder Health(int health)
        {
            _creatureProtoData.Health = health;

            return this;
        }

        public ICreatureBuilder Defense(int defense)
        {
            _creatureProtoData.Defense = defense;

            return this;
        }

        public ICreatureBuilder Name(string name)
        {
            _creatureProtoData.Name = name;

            return this;
        }

        public ICreatureBuilder Art(string uri)
        {
            _creatureProtoData.MetaData = _creatureProtoData.MetaData with { ImageUri = uri };

            return this;
        }

        public ICreatureBuilder Level(int level)
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