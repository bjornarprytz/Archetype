using Archetype.Builder.Builders;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Builder.Extensions;
using Archetype.Builder.Factory;
using Archetype.Design.Extensions;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.View.Primitives;

namespace Archetype.Design
{
    public interface IDesign
    {
        void Create(); // TODO: This is placeholder. Don't just return void
    }

    public class TestDesign : IDesign
    {
        private readonly IProtoPool _protoPool;
        private readonly IBuilderFactory _builderFactory;
        private readonly IMap _map;
        private readonly IPlayerData _playerData;


        public TestDesign(IProtoPool protoPool, IBuilderFactory builderFactory, IMap map, IPlayerData playerData)
        {
            _protoPool = protoPool;
            _builderFactory = builderFactory;
            _map = map;
            _playerData = playerData;
        }


        public void Create()
        {
            CreateSet();
            CreateMap();

            _playerData.SubmitDeck(_protoPool.Cards.PickNUnique(5));
        }
        
        private void CreateSet()
        {
            _protoPool
                .AddSet("All rares", _builderFactory, builder => builder
                    .ChangeCardTemplate(t => t with { Rarity = CardRarity.Rare})
                    .Card(cardBuilder => cardBuilder
                        .Name("Cost reducer")
                        .Range(1)
                        .Effect<ICard>(context => context.Target.ReduceCost(1)))
                    .Card(cardBuilder => cardBuilder
                        .Name("Health dealer")
                        .Range(1)
                        .Effect<IUnit>(context => context.Target.Attack(context.Target.Health)))
                    .Creature(creatureBuilder => creatureBuilder
                        .Name("Ghoul")
                        .Strength(1)
                        .Health(2))
                    .Structure(structureBuilder => structureBuilder
                        .Name("House")
                        .Defense(1)
                        .Health(1))
                    .Card(cardBuilder => cardBuilder
                        .Name("Create Unit")
                        .Range(0)
                        .Effect<IMapNode>(context => context.Target.CreateCreature("Ghoul", context.Owner))))
                .AddSet("TestSet", _builderFactory, 
                    setProvider => setProvider
                        .ChangeCardTemplate(t => t with { Color = CardColor.Black })
                        .Card(builder =>
                            builder
                                .Name("Slap heal")
                                .Cost(4)
                                .Range(0)
                                .Targets<IUnit, IUnit>()
                                .Effect<IUnit>(context => context.Target.Attack(5), targetIndex:0)
                                .Effect<IUnit>(context => context.Target.Heal(context.DamageDealt()), targetIndex:1)
                                .Art("asd")
                        )
                        .Card(builder =>
                            builder
                                .Name("Resource slap")
                                .Cost(3)
                                .Range(1)
                                .Effect<ICreature>(context => context.Target.Attack(4))
                                .Art("other")
                        )
                        .Card(builder =>
                            builder
                                .Red()
                                .Name("Slap units")
                                .Cost(1)
                                .Range(1)
                                .Effect<IZone<IUnit>>(context => context.UnitsInTargetZone().TargetEach(unit => unit.Attack(3)))
                                .Art("other")
                        )
                        .Card(builder =>
                            builder
                                .Blue()
                                .Name("Slap cards in hand")
                                .Cost(1)
                                .Range(1)
                                .Effect(context => context.CardsInPlayersHand().TargetEach(card => card.ReduceCost(1)))
                                .Art("other")
                        ));
        }

        private void CreateMap()
        {
            _map.Generate(
                _builderFactory.Create<IMapBuilder>()
                    .Nodes(3)
                    .Connect(0,2)
                    .Connect(2,1)
                    .Build());

        }
    }
    
}

