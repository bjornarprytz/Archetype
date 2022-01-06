using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.Builder.Builders.Base;
using Archetype.Builder.Factory;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Builder.Builders
{
    public interface IMapBuilder : IBuilder<IMapProtoData>
    {
        IMapBuilder Node(Action<INodeBuilder> builderProvider);
        
        IMapBuilder Connect(int n1, int n2); 
        IMapBuilder Connect(string n1, string n2);
    }

    internal class MapBuilder : ProtoBuilder<IMapProtoData>, IMapBuilder
    {
        private readonly IFactory<INodeBuilder> _nodeBuilderFactory;
        private readonly List<IMapNodeProtoData> _nodes = new();

        private readonly Dictionary<int, List<int>> _connections = new();
        private readonly Dictionary<int, IReadOnlyList<int>> _finalConnections = new();

        private readonly IMapProtoData _mapProtoData;
        
        public MapBuilder(IFactory<INodeBuilder> nodeBuilderFactory)
        {
            _nodeBuilderFactory = nodeBuilderFactory;
            _mapProtoData = new MapProtoData(_nodes, _finalConnections);
        }

        public IMapBuilder Node(Action<INodeBuilder> builderProvider)
        {
            var builder = _nodeBuilderFactory.Create();

            builderProvider(builder);

            _nodes.Add(builder.Build());

            return this;
        }

        public IMapBuilder Connect(int n1, int n2)
        {
            if (n1 == n2)
                throw new ArgumentException("Can't connect a node to itself");
            
            (_connections[n1] ??= new List<int>()).Add(n2);

            return this;
        }

        public IMapBuilder Connect(string n1, string n2)
        {
            var node1 = _nodes.FirstOrDefault(n => n.Name == n1);
            var node2 = _nodes.FirstOrDefault(n => n.Name == n2);
            
            var index1 = _nodes.IndexOf(node1);
            var index2 = _nodes.IndexOf(node2);

            return Connect(index1, index2);
        }

        protected override IMapProtoData BuildInternal()
        {
            foreach (var (nodeIndex, connections) in _connections)
            {
                if (nodeIndex > _nodes.Count)
                    continue;
                
                var neighbours = connections
                    .Distinct()
                    .Where(idx => idx <= _nodes.Count)
                    .ToList(); 
                
                _finalConnections[nodeIndex] = neighbours;
            }
            
            _connections.Clear();
            
            return _mapProtoData;
        }
    }
}