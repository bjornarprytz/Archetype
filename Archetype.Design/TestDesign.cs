using Archetype.Builder.Builders;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;
using Archetype.Builder.Extensions;
using Archetype.Builder.Factory;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Primitives;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Design
{
    public static class TestDesign
    {
        public static IProtoPool BuildCardPool(IPoolBuilder poolBuilder)
        {
            return new ProtoPool(new List<ISet>());
            
            return poolBuilder
                .AddSet("All rares", builder => builder
                    .ChangeCardTemplate(t => t with { Rarity = CardRarity.Rare})
                    .Card(cardBuilder => cardBuilder
                        .Name("Cost reducer")
                        .Effect<ICard>(context => context.Target.ReduceCost(1))
                        )
                    .Card(cardBuilder => cardBuilder
                        .Name("Health dealer")
                        .Effect<IUnit>(context => context.Target.Attack(context.Target.Health)))
                    .Creature(creatureBuilder => creatureBuilder
                        .Name("Ghoul")
                        .Strength(1)
                        .Health(2))
                    .Card(cardBuilder => cardBuilder
                        .Name("Create Unit")
                        .Effect<IMapNode>(context => context.Target.CreateCreature("Ghoul", context.Owner))))
                .AddSet("TestSet", 
                    setProvider => setProvider
                        .ChangeCardTemplate(t => t with { Color = CardColor.Black })
                        .Card(builder =>
                            builder
                                .Name("Slap heal")
                                .Cost(4)
                                .Targets<IUnit, IUnit>()
                                .Effect<IUnit>(context => context.Target.Attack(5), targetIndex:0)
                                .Effect<IUnit>(context => context.Target.Heal(context.DamageDealt()), targetIndex:1)
                                .Art("asd")
                        )
                        .Card(builder =>
                            builder
                                .Name("Resource slap")
                                .Cost(3)
                                .Effect<ICreature>(context => context.Target.Attack(4))
                                .Art("other")
                        )
                        .Card(builder =>
                            builder
                                .Red()
                                .Name("Slap units")
                                .Cost(1)
                                .Effect<IZone<IUnit>>(context => context.UnitsInTargetZone().TargetEach(unit => unit.Attack(3)))
                                .Art("other")
                        )
                        .Card(builder =>
                            builder
                                .Blue()
                                .Name("Slap cards in hand")
                                .Cost(1)
                                .Effect(context => context.CardsInPlayersHand().TargetEach(card => card.ReduceCost(1)))
                                .Art("other")
                        ))
                .Build();
        }

        public static IMapProtoData BuildMap(IMapBuilder mapBuilder)
        {
            return new MapProtoData(new List<IMutableMapNode>());
            
            return mapBuilder
                .Nodes(3)
                .Connect(0,2)
                .Connect(2,1)
                .Build();

        }
    }
    
}

