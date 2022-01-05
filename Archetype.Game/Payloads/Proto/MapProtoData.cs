using System.Collections.Generic;
using Archetype.Game.Payloads.Atoms;
using Archetype.View.Proto;

namespace Archetype.Game.Payloads.Proto
{
    public interface IMapProtoData : IProtoDataFront
    {
        // TODO: These should be Connections and NodeProtoData, not actual nodes
        IEnumerable<IMapNode> Nodes { get; } 
    }

    public class MapProtoData : ProtoData, IMapProtoData
    {
        private readonly List<IMutableMapNode> _nodes;

        public MapProtoData(List<IMutableMapNode> nodes)
        {
            _nodes = nodes;
        }
        
        public IEnumerable<IMapNode> Nodes => _nodes;
    }
}