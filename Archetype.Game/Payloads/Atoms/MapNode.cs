using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using Archetype.Game.Attributes;
using Archetype.Game.Factory;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.View.Atoms;
using Archetype.View.Atoms.Zones;
using Archetype.View.Events;

namespace Archetype.Game.Payloads.Atoms
{
    public interface IMapNode : IZone<IUnit>, IMapNodeFront
    {
        new IObservable<IAtomMutation<IMapNode>> OnMutation { get; }
        new IEnumerable<IMapNode> Neighbours { get; }
        new IGraveyard Graveyard { get; }
        new IDiscardPile DiscardPile { get; }

        IResult<IMapNode, ICreature> Spawn(string name, IGameAtom owner);
        IResult<IMapNode, IStructure> Build(string name, IGameAtom owner);

        IResult<IMapNode, IMapNode> ConnectTo(IMapNode other);

    }

    internal class MapNode : Zone<IUnit>, IMapNode
    {
        private readonly IInstanceFactory _instanceFactory;
        private readonly Dictionary<Guid, IMapNode> _neighbours = new();
        private readonly Subject<IAtomMutation<IMapNode>> _mutation = new();

        public MapNode(IGameAtom owner, IInstanceFactory instanceFactory) : base(owner)
        {
            _instanceFactory = instanceFactory;
            DiscardPile = new DiscardPile(this);
            Graveyard = new Graveyard(this);
        }
        
        public IEnumerable<IUnitFront> Units => Contents;

        IObservable<IAtomMutation<IMapNode>> IMapNode.OnMutation => _mutation;
        public override IObservable<IAtomMutation> OnMutation => _mutation;
        public IEnumerable<IMapNode> Neighbours => _neighbours.Values;
        public IGraveyard Graveyard { get; }
        public IDiscardPile DiscardPile { get; }
        IEnumerable<IMapNodeFront> IMapNodeFront.Neighbours => Neighbours;
        IGraveyardFront IMapNodeFront.Graveyard => Graveyard;
        IDiscardPileFront IMapNodeFront.DiscardPile => DiscardPile;
        public IResult<IMapNode, ICreature> Spawn(string name, IGameAtom owner)
        {
            var creature = _instanceFactory.CreateCreature(name, owner); 
            
            creature.MoveTo(this);

            return ResultFactory.Create(this, creature);
        }

        public IResult<IMapNode, IStructure> Build(string name, IGameAtom owner)
        {
            var creature = _instanceFactory.CreateStructure(name, owner); 
            
            creature.MoveTo(this);

            return ResultFactory.Create(this, creature);
        }

        public IResult<IMapNode, IMapNode> ConnectTo(IMapNode other)
        {
            if (_neighbours.ContainsKey(other.Guid))
                return ResultFactory.Null<IMapNode, IMapNode>(this);
            
            _neighbours.Add(other.Guid, other);
            other.ConnectTo(this);

            return ResultFactory.Create(this, other);
        }
    }
}