using System.Collections.Generic;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.Game.Payloads.Proto
{
    public interface IMapProtoData
    {
        IEnumerable<IMapNode> Nodes { get; }
    }
    
    public class MapProtoData : IMapProtoData
    {
        private readonly List<IMutableMapNode> _nodes;

        public MapProtoData(List<IMutableMapNode> nodes)
        {
            _nodes = nodes;
        }
        
        public IEnumerable<IMapNode> Nodes => _nodes;
    }
}