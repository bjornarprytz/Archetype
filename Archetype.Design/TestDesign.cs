using Aqua.EnumerableExtensions;
using Archetype.Dto.Simple;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;
using Archetype.Builder.Extensions;
using Archetype.Builder.Factory;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Design
{
    public static class TestDesign
    {
        public static ICardPool BuildCardPool()
        {
            return BuilderFactory.CardPoolBuilder()
                .AddSet("All rares", builder => builder
                    .ChangeTemplate(t => t with { Rarity = CardRarity.Rare})
                    .Card(cardBuilder => cardBuilder
                        .Name("Cost reducer")
                        .Target<ICard>()
                        .Effect<ICard>(
                            targetIndex: 0,
                            resolveEffect: context => context.Target.ReduceCost(1))
                        ))
                .AddSet("TestSet", 
                    setProvider => setProvider
                        .ChangeTemplate(t => t with { Color = CardColor.Black })
                        .Card(builder =>
                            builder
                                .Name("Slap heal")
                                .Cost(4)
                                .Target<IUnit>()
                                .Attack(5, 0)
                                .Effect(resolveEffect: context => context.GameState.Player.Hand.Contents.ForEach((card, i) => card.ReduceCost(i)))
                                .Art("asd")
                        )
                        .Card(builder =>
                            builder
                                .Name("Resource slap")
                                .Cost(3)
                                .Target<IUnit>()
                                .Effect<IUnit>(
                                    targetIndex: 0,
                                    resolveEffect: context => context.Target.Attack(context.GameState.Player.Resources))
                                .Art("other")
                        )
                        .Card(builder =>
                            builder
                                .Red()
                                .Name("Slap cards")
                                .Cost(1)
                                .Target<IZone<ICard>>()
                                .Effect<IZone<ICard>>(
                                    targetIndex: 0,
                                    resolveEffect: context => context.Target.Contents.ForEach((card, i) => card.ReduceCost(i)))
                                .Art("other")
                        )
                        .Card(builder =>
                            builder
                                .Blue()
                                .Name("Slap all")
                                .Cost(1)
                                .Effect(
                                    resolveEffect: context => context.GameState.Player.Hand.Contents.ForEach((card, i) => card.ReduceCost(i)))
                                .Art("other")
                        ))
                .Build();
        }

        public static IMapProtoData BuildMap()
        {
            return BuilderFactory.MapBuilder()
                .Nodes(3)
                .Connect(0,2)
                .Connect(2,1)
                .Build();

        }
    }
    
}

