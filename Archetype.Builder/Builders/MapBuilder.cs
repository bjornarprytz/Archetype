using System.Collections.Generic;
using System.Linq;
using Aqua.EnumerableExtensions;
using Archetype.Builder.Builders.Base;
using Archetype.Builder.Exceptions;
using Archetype.Game.Payloads.Proto;
using Archetype.Game.Extensions;

namespace Archetype.Builder.Builders
{
    public interface IMapBuilder : IBuilder<IMapProtoData>
    {
        public IMapBuilder Nodes(int numberOfNodes);
        public IMapBuilder Connect(int n1, int n2);
    }

    internal class MapBuilder : IMapBuilder
    {
        private readonly List<IMapNodeProtoData> _nodes = new();

        private readonly IMapProtoData _mapProtoData;

        private readonly Dictionary<int, List<int>> _connections = new();

        public MapBuilder()
        {
            _mapProtoData = new MapProtoData(_nodes);
        }

        public IMapBuilder Nodes(int numberOfNodes)
        {
            for (var i = 0; i < numberOfNodes; i++)
            {
                var node = new MapNodeProtoData(i, _connections.GetOrSet(i));
                
                _nodes.Add(node);
            }

            return this;
        }

        public IMapBuilder Connect(int n1, int n2)
        {
            if (n1 == n2)
                return this;
            
            _connections
                .GetOrSet(n1).Add(n2);

            return this;
        }
        
        public IMapProtoData Build()
        {
            if (_nodes.Count == 1 || _nodes.Any(node => _connections.All((conns => conns.Key != node.Id && conns.Value.All(id => id != node.Id)))))
                throw new DisconnectedNodesException();
            
            return _mapProtoData;
        }
    }
}