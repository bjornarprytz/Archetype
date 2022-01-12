using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Archetype.Builder.Builders.Base;
using Archetype.Builder.Exceptions;
using Archetype.Builder.Extensions;
using Archetype.Builder.Factory;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Builder.Builders
{
    public interface IMapBuilder : IBuilder<IMapProtoData>
    {
        // TODO: Make this interface nice for building nodes and neighbours. Remember; Nodes have names, which should be unique
        
        public IMapBuilder Node(Action<INodeBuilder> builderProvider);
        
        public IMapBuilder Connect(string n1, string n2);
    }

    internal class MapBuilder : ProtoBuilder<IMapProtoData>, IMapBuilder
    {
        private readonly IFactory<INodeBuilder> _nodeBuilderFactory;
        private readonly Dictionary<string, IMapNodeProtoData> _nodes = new();

        private readonly IMapProtoData _mapProtoData;
        
        public MapBuilder(IFactory<INodeBuilder> nodeBuilderFactory)
        {
            _nodeBuilderFactory = nodeBuilderFactory;
            _mapProtoData = new MapProtoData(_nodes);
        }

        public IMapBuilder Node(Action<INodeBuilder> builderProvider)
        {
            var builder = _nodeBuilderFactory.Create();

            builderProvider(builder);

            var node = builder.Build();
            
            _nodes.Add(node.Name, node);

            return this;
        }

        public IMapBuilder Connect(string n1, string n2)
        {
            var node1 = _nodes[n1];
            var node2 = _nodes[n2];
            
            node1.DuplexConnection(node2);

            return this;
        }
        
        protected override IMapProtoData BuildInternal()
        {
            if (_nodes.Count > 1 && _nodes.Values.Any(node => node.Neighbours.IsEmpty()))
                throw new DisconnectedNodesException();
            
            return _mapProtoData;
        }
    }
}