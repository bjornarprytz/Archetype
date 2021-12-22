using System.Collections.Generic;
using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Infrastructure
{
    public interface IMapFront
    {
        IEnumerable<IMapNodeFront> Nodes { get; }
    }
    
    [Target("Map")]
    internal interface IMap : IMapFront
    {
        new IEnumerable<IMapNode> Nodes { get; } 

        void Generate(IMapProtoData protoData);
    }

    internal class Map : IMap
    {
        private readonly List<IMapNode> _nodes = new();

        public IEnumerable<IMapNode> Nodes => _nodes;
        public void Generate(IMapProtoData protoData)
        {
            _nodes.AddRange(protoData.Nodes);
        }

        IEnumerable<IMapNodeFront> IMapFront.Nodes => Nodes;
    }
}
