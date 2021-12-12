using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.Builder.Exceptions;
using Archetype.Builder.Extensions;
using Archetype.Builder.Factory;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Builder.Builders
{
    public class MapBuilder : IBuilder<IMapProtoData>
    {
        private readonly List<IMutableMapNode> _nodes = new();

        private readonly IMapProtoData _mapProtoData;
        
        internal MapBuilder()
        {
            _mapProtoData = new MapProtoData(_nodes);
        }

        public MapBuilder Node(Action<NodeBuilder> builderProvider)
        {
            var builder = BuilderFactory.NodeBuilder();

            builderProvider(builder);
            
            _nodes.Add(builder.Build());

            return this;
        }

        public MapBuilder Nodes(int numberOfNodes)
        {
            for (var i = 0; i < numberOfNodes; i++)
            {
                var builder = BuilderFactory.NodeBuilder();
                
                _nodes.Add(builder.Build());
            }

            return this;
        }

        public MapBuilder Connect(int n1, int n2)
        {
            var node1 = _nodes.ElementAt(n1);
            var node2 = _nodes.ElementAt(n2);
            
            node1.AddNeighbour(node2);

            return this;
        }
        
        public IMapProtoData Build()
        {
            if (_nodes.Count > 1 && _nodes.Any(node => node.Neighbours.IsEmpty()))
                throw new DisconnectedNodesException();
            
            return _mapProtoData;
        }
    }
}