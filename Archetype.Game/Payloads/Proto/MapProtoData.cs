using System.Collections.Generic;
using Archetype.View.Proto;

namespace Archetype.Game.Payloads.Proto
{
    public interface IMapProtoData : IProtoDataFront
    {
        IEnumerable<IMapNodeProtoData> Nodes { get; }
    }

    public class MapProtoData : ProtoData, IMapProtoData
    {
        private readonly List<IMapNodeProtoData> _nodes;

        public MapProtoData(List<IMapNodeProtoData> nodes)
        {
            _nodes = nodes;
        }
        
        public IEnumerable<IMapNodeProtoData> Nodes => _nodes;
    }

    public interface IMapNodeProtoData
    {
        int Id { get; }
        IEnumerable<int> Connections { get; }
    }

    public class MapNodeProtoData : IMapNodeProtoData
    {
        private readonly List<int> _connections;

        public MapNodeProtoData(int id, List<int> connections)
        {
            Id = id;
            _connections = connections;
        }
        
        public int Id { get; }
        public IEnumerable<int> Connections => _connections;
    }
}