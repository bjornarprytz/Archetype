using Archetype.Builder.Builders;
using Archetype.Builder.Extensions;
using Archetype.Builder.Factory;
using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Base;
using Archetype.Core.Extensions;
using Archetype.Core.Infrastructure;
using Archetype.Core.Proto;
using Archetype.Design.Extensions;
using Archetype.View.Primitives;

namespace Archetype.Design;

public interface IDesign
{
    void Create(); // TODO: This is placeholder. Don't just return void
}

public class TestDesign : IDesign
{
    private readonly IProtoPool _protoPool;
    private readonly IInstanceFactory _instanceFactory;
    private readonly IFactory<ISetBuilder> _setBuilderFactory;
    private readonly IFactory<IMapBuilder> _mapBuilderFactory;
    private readonly IMap _map;
    private readonly IPlayerData _playerData;


    public TestDesign(
        IProtoPool protoPool,
        IInstanceFactory instanceFactory,
        IFactory<ISetBuilder> setBuilderFactory,
        IFactory<IMapBuilder> mapBuilderFactory,
        IMap map,
        IPlayerData playerData)
    {
        _protoPool = protoPool;
        _instanceFactory = instanceFactory;
        _setBuilderFactory = setBuilderFactory;
        _mapBuilderFactory = mapBuilderFactory;
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
            .AddSet("All rares", _setBuilderFactory, builder => builder
                .ChangeCardTemplate(t => t with { Rarity = CardRarity.Rare})
                .Card(cardBuilder => cardBuilder
                    .Name("Cost reducer")
                    .Range(1)
                    .Effect(context => context.Target<ICard>().ReduceCost(1)))
                .Card(cardBuilder => cardBuilder
                    .Name("Health dealer")
                    .Range(1)
                    .Effect(context => context.Target<IUnit>(0).Attack(context.Target<IUnit>(1).Health)))
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
                    .Effect(context => context.Target<IMapNode>().Spawn("Ghoul", context.Source.Owner))))
            .AddSet("TestSet", _setBuilderFactory, 
                setProvider => setProvider
                    .ChangeCardTemplate(t => t with { Color = CardColor.Black })
                    .Card(builder =>
                        builder
                            .Name("Slap heal")
                            .Cost(4)
                            .Range(0)
                            .Effect(context => context.Target<IUnit>().Attack(5))
                            .Effect(context => context.Target<IUnit>().Heal(context.DamageDealt()))
                            .Art("asd")
                    )
                    .Card(builder =>
                        builder
                            .Name("Resource slap")
                            .Cost(3)
                            .Range(1)
                            .Effect(context => context.Target<ICreature>().Attack(4))
                            .Art("other")
                    )
                    .Card(builder =>
                        builder
                            .Red()
                            .Name("Slap units")
                            .Cost(1)
                            .Range(1)
                            .Effect(context => context.Target<IZone<IUnit>>().Contents.TargetEach(unit => unit.Attack(3)))
                            .Art("other")
                    )
                    .Card(builder =>
                        builder
                            .Blue()
                            .Name("Slap cards in hand")
                            .Cost(1)
                            .Range(1)
                            .Effect(context => context.GameState.Player.Hand.Contents.TargetEach(card => card.ReduceCost(1)))
                            .Art("other")
                    ));
    }

    private void CreateMap()
    {
        _map.AddNodes(Generate(
            _mapBuilderFactory.Create()
                .Nodes(3)
                .Connect(0,2)
                .Connect(2,1)
                .Build()));

    }

    private IEnumerable<IMapNode> Generate(IMapProtoData protoData)
    {
        var protoDatas = protoData.Nodes.ToArray();
            
        var mapNodes = protoData.Nodes.Select(proto => _instanceFactory.CreateMapNode(proto)).ToArray();

        foreach (var (mapNode, i) in mapNodes.Select((node, i) => (node, i)))
        {
            foreach (var neighbour in protoDatas[i].Connections)
            {
                mapNode.ConnectTo(mapNodes[neighbour]); // TODO: History log this?
            }
        }

        return mapNodes;
    }
}