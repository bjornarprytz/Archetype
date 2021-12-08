using System;
using System.Collections.Generic;
using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Pieces
{
    [Target("Node")]
    public interface IMapNode : IZone<IUnit>
    {
        IEnumerable<IMapNode> Neighbours { get; }
    }

    public interface IMutableMapNode : IMapNode
    {
        void AddNeighbour(IMutableMapNode node);
        void RemoveNeighbour(IMutableMapNode node);
    }
    
    public class MapNode : Zone<IUnit>, IMutableMapNode
    {
        private readonly Dictionary<Guid, IMapNode> _neighbours = new();
        public MapNode(IGameAtom owner=default) : base(owner) { }
        
        public IEnumerable<IMapNode> Neighbours => _neighbours.Values;

        public void AddNeighbour(IMutableMapNode node)
        {
            if (_neighbours.ContainsKey(node.Guid))
                return;
            
            _neighbours.Add(node.Guid, node);
            node.AddNeighbour(this);
        }

        public void RemoveNeighbour(IMutableMapNode node)
        {
            if (!_neighbours.ContainsKey(node.Guid))
                return;

            _neighbours.Remove(node.Guid);
            node.RemoveNeighbour(this);
        }
    }
}