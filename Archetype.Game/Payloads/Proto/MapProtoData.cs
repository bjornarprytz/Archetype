using System.Collections.Generic;
using Archetype.View.Proto;

namespace Archetype.Game.Payloads.Proto
{
    public interface IMapNodeProtoData : IMapNodeProtoDataFront
    {
        new IEnumerable<IMapNodeProtoData> Neighbours { get; }

        new int MaxStructures { get; set; }
        void DuplexConnection(IMapNodeProtoData other);
        void OneWayConnection(IMapNodeProtoData other);
    }

    public interface IMapProtoData : IMapProtoDataFront
    {
        new IEnumerable<IMapNodeProtoData> Nodes { get; }
    }

    public class MapProtoData : ProtoData, IMapProtoData
    {
        private readonly Dictionary<string, IMapNodeProtoData> _nodes;

        public MapProtoData(Dictionary<string, IMapNodeProtoData> nodes)
        {
            _nodes = nodes;
        }


        public IEnumerable<IMapNodeProtoData> Nodes => _nodes.Values;

        IEnumerable<IMapNodeProtoDataFront> IMapProtoDataFront.Nodes => Nodes;
    }

    public class MapNodeProtoData : ProtoData, IMapNodeProtoData
    {
        private readonly List<IMapNodeProtoData> _neighbours = new();

        public int MaxStructures { get; set; }
        public IEnumerable<IMapNodeProtoData> Neighbours => _neighbours;
        public void DuplexConnection(IMapNodeProtoData other)
        {
            OneWayConnection(other);
            other.OneWayConnection(this);
        }

        public void OneWayConnection(IMapNodeProtoData other)
        {
            if (_neighbours.Contains(other))
                return;
            
            _neighbours.Add(other);
        }

        IEnumerable<IMapNodeProtoDataFront> IMapNodeProtoDataFront.Neighbours => Neighbours;
    }
}