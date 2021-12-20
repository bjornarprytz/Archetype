using System.Collections.Generic;
using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Infrastructure
{
    [Target("Map")]
    public interface IMap
    {
        IEnumerable<IMapNode> Nodes { get; }

        void Generate(IMapProtoData protoData);
    }

    public class Map : IMap
    {
        private readonly List<IMapNode> _nodes = new();

        public IEnumerable<IMapNode> Nodes => _nodes;
        public void Generate(IMapProtoData protoData)
        {
            _nodes.AddRange(protoData.Nodes);
        }
    }
}
