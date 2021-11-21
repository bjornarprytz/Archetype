using System;
using System.Collections.Generic;
using Aqua.TypeExtensions;
using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Pieces
{
    [Target("Node")]
    public interface IMapNode : IZone<IUnit>
    {
        IEnumerable<IMapNode> Neighbours { get; }

        void AddNeighbour(IMapNode node);
        void RemoveNeighbour(IMapNode node);
    }
    
    public class MapNode : Zone<IUnit>, IMapNode
    {
        private readonly Dictionary<Guid, IMapNode> _neighbours = new();
        public MapNode(IGameAtom owner=default) : base(owner)
        {
        }
        
        public IEnumerable<IMapNode> Neighbours => _neighbours.Values;

        public void AddNeighbour(IMapNode node)
        {
            if (_neighbours.ContainsKey(node.Guid))
                return;
            
            _neighbours.Add(node.Guid, node);
            node.AddNeighbour(this);
        }

        public void RemoveNeighbour(IMapNode node)
        {
            if (!_neighbours.ContainsKey(node.Guid))
                return;

            _neighbours.Remove(node.Guid);
            node.RemoveNeighbour(this);
        }
    }
}