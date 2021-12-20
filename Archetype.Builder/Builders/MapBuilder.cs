using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.Builder.Builders.Base;
using Archetype.Builder.Exceptions;
using Archetype.Builder.Extensions;
using Archetype.Builder.Factory;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Builder.Builders
{
    public interface IMapBuilder : IBuilder<IMapProtoData>
    {
        public IMapBuilder Node(Action<INodeBuilder> builderProvider);
        public IMapBuilder Nodes(int numberOfNodes);
        public IMapBuilder Connect(int n1, int n2);
    }

    public class MapBuilder : IMapBuilder
    {
        private readonly IBuilderFactory _builderFactory;
        private readonly List<IMutableMapNode> _nodes = new();

        private readonly IMapProtoData _mapProtoData;
        
        public MapBuilder(IBuilderFactory builderFactory)
        {
            _builderFactory = builderFactory;
            _mapProtoData = new MapProtoData(_nodes);
        }

        public IMapBuilder Node(Action<INodeBuilder> builderProvider)
        {
            var builder = _builderFactory.Create<INodeBuilder>();

            builderProvider(builder);
            
            _nodes.Add(builder.Build());

            return this;
        }

        public IMapBuilder Nodes(int numberOfNodes)
        {
            for (var i = 0; i < numberOfNodes; i++)
            {
                var builder = _builderFactory.Create<INodeBuilder>();
                
                _nodes.Add(builder.Build());
            }

            return this;
        }

        public IMapBuilder Connect(int n1, int n2)
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