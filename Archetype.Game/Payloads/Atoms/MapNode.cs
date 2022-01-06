using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Attributes;
using Archetype.Game.Factory;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Proto;
using Archetype.View.Atoms;
using Archetype.View.Atoms.Zones;

namespace Archetype.Game.Payloads.Atoms
{
    [Target("Node")]
    public interface IMapNode : IZone<IUnit>, IMapNodeFront
    {
        new IEnumerable<IMapNode> Neighbours { get; }
        new IGraveyard Graveyard { get; }
        new IDiscardPile DiscardPile { get; }

        [Template("Create {1} at {0}, owned by {2}")]
        IResult<IMapNode, ICreature> CreateCreature(string name, IGameAtom owner);
        [Template("Create {1} at {0}, owned by {2}")]
        IResult<IMapNode, IStructure> CreateStructure(string name, IGameAtom owner);

        internal void AddNeighbour(IMapNode node);
        internal void RemoveNeighbour(IMapNode node); 
    }

    internal class MapNode : Zone<IUnit>, IMapNode
    {
        private readonly IInstanceFactory _instanceFactory;
        private readonly Dictionary<Guid, IMapNode> _neighbours = new();

        public MapNode(IMapNodeProtoData protoData, IGameAtom owner, IInstanceFactory instanceFactory) : base(owner)
        {
            Name = protoData.Name;
            MaxStructures = protoData.MaxStructures;
            _instanceFactory = instanceFactory;
            DiscardPile = new DiscardPile(this);
            Graveyard = new Graveyard(this);
        }

        public int MaxStructures { get; }
        
        public IEnumerable<IUnitFront> Units => Contents;
        public IEnumerable<IMapNode> Neighbours => _neighbours.Values;
        public IGraveyard Graveyard { get; }
        public IDiscardPile DiscardPile { get; }
        IEnumerable<IMapNodeFront> IMapNodeFront.Neighbours => Neighbours;
        IGraveyardFront IMapNodeFront.Graveyard => Graveyard;
        IDiscardPileFront IMapNodeFront.DiscardPile => DiscardPile;
        public IResult<IMapNode, ICreature> CreateCreature(string name, IGameAtom owner)
        {
            var creature = _instanceFactory.CreateCreature(name, owner); 
            
            creature.MoveTo(this);

            return ResultFactory.Create(this, creature);
        }

        public IResult<IMapNode, IStructure> CreateStructure(string name, IGameAtom owner)
        {
            var creature = _instanceFactory.CreateStructure(name, owner); 
            
            creature.MoveTo(this);

            return ResultFactory.Create(this, creature);
        }

        void IMapNode.AddNeighbour(IMapNode node)
        {
            if (_neighbours.ContainsKey(node.Guid))
                return;
        
            _neighbours.Add(node.Guid, node);
            node.AddNeighbour(this);
        }

        void IMapNode.RemoveNeighbour(IMapNode node)
        {
            if (!_neighbours.ContainsKey(node.Guid))
                return;

            _neighbours.Remove(node.Guid);
            node.RemoveNeighbour(this);
        }
    }
}