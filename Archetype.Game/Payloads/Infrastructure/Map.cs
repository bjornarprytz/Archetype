using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Infrastructure
{
    public interface IMap
    {
        IEnumerable<IMapNode> Nodes { get; }
    }

    public class Map : IMap
    {
        private readonly List<IMapNode> _nodes;

        public Map(IMapProtoData protoData)
        {
            _nodes = protoData.Nodes.ToList();
        }
        
        public IEnumerable<IMapNode> Nodes => _nodes;
    }
}
