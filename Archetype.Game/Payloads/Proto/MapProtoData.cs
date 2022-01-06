using System.Collections.Generic;
using Archetype.View.Proto;

namespace Archetype.Game.Payloads.Proto
{
    public interface IMapNodeProtoData : IMapNodeProtoDataFront
    {
        new int MaxStructures { get; set; }
    }

    public interface IMapProtoData : IMapProtoDataFront
    {
        new IEnumerable<IMapNodeProtoData> Nodes { get; }
        IReadOnlyDictionary<int, IReadOnlyList<int>> Connections { get; }
    }

    public class MapProtoData : ProtoData, IMapProtoData
    {
        private readonly List<IMapNodeProtoData> _nodes;
        private readonly Dictionary<int, IReadOnlyList<int>> _connections;

        public MapProtoData(List<IMapNodeProtoData> nodes, Dictionary<int, IReadOnlyList<int>> connections)
        {
            _nodes = nodes;
            _connections = connections;
        }
        public IEnumerable<IMapNodeProtoData> Nodes => _nodes;
        public IReadOnlyDictionary<int, IReadOnlyList<int>> Connections => _connections;
        
        IEnumerable<IMapNodeProtoDataFront> IMapProtoDataFront.Nodes => Nodes;
    }

    public class MapNodeProtoData : ProtoData, IMapNodeProtoData
    {
        public int MaxStructures { get; set; }
    }
}