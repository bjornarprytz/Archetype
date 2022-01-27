using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Proto;
using Archetype.View;
using Archetype.View.Atoms.Zones;
using Archetype.View.Infrastructure;

namespace Archetype.Game.Payloads.Infrastructure
{
    public interface IMap : IMapFront
    {
        new IEnumerable<IMapNode> Nodes { get; } 

        void AddNodes(IEnumerable<IMapNode> nodes);
    }

    internal class Map : IMap
    {
        private readonly List<IMapNode> _nodes = new();

        public IEnumerable<IMapNode> Nodes => _nodes;
        public void AddNodes(IEnumerable<IMapNode> nodes)
        {
            _nodes.AddRange(nodes);
        }

        IEnumerable<IMapNodeFront> IMapFront.Nodes => Nodes;
    }
}
