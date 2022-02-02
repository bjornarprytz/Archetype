using Archetype.Core.Atoms.Base;
using Archetype.Core.Infrastructure;
using Archetype.Core.Play;
using Archetype.View.Atoms;
using Archetype.View.Atoms.Zones;
using Archetype.View.Infrastructure;

namespace Archetype.Core.Atoms;

public interface IMapNode : IZone<IUnit>, IMapNodeFront
{
    new IEnumerable<IMapNode> Neighbours { get; }
    new IGraveyard Graveyard { get; }
    new IDiscardPile DiscardPile { get; }

    IEffectResult<IMapNode, ICreature> Spawn(string name, IGameAtom owner);
    IEffectResult<IMapNode, IStructure> Build(string name, IGameAtom owner);

    IEffectResult<IMapNode, IMapNode> ConnectTo(IMapNode other);

}

internal class MapNode : Zone<IUnit>, IMapNode
{
    private readonly IInstanceFactory _instanceFactory;
    private readonly Dictionary<Guid, IMapNode> _neighbours = new();

    public MapNode(IInstanceFactory instanceFactory)
    {
        _instanceFactory = instanceFactory;
        DiscardPile = new DiscardPile(this);
        Graveyard = new Graveyard(this);
    }
        
    public IEnumerable<IUnitFront> Units => Contents;
    public IEnumerable<IMapNode> Neighbours => _neighbours.Values;
    public IGraveyard Graveyard { get; }
    public IDiscardPile DiscardPile { get; }
    IEnumerable<IMapNodeFront> IMapNodeFront.Neighbours => Neighbours;
    IGraveyardFront IMapNodeFront.Graveyard => Graveyard;
    IDiscardPileFront IMapNodeFront.DiscardPile => DiscardPile;
    public IEffectResult<IMapNode, ICreature> Spawn(string name, IGameAtom owner)
    {
        var creature = _instanceFactory.CreateCreature(name); 
            
        var sideEffects = new List<IEffectResult>
        {
            creature.SetOwner(owner),
            creature.MoveTo(this)
        };

        return ResultFactory.Create(this, creature, sideEffects);
    }

    public IEffectResult<IMapNode, IStructure> Build(string name, IGameAtom owner)
    {
        var structure = _instanceFactory.CreateStructure(name);

        var sideEffects = new List<IEffectResult>
        {
            structure.SetOwner(owner),
            structure.MoveTo(this)
        };

        return ResultFactory.Create(this, structure, sideEffects);
    }

    public IEffectResult<IMapNode, IMapNode> ConnectTo(IMapNode other)
    {
        if (_neighbours.ContainsKey(other.Guid))
            return ResultFactory.Null<IMapNode, IMapNode>();
            
        _neighbours.Add(other.Guid, other);
        other.ConnectTo(this);

        return ResultFactory.Create(this, other);
    }
}