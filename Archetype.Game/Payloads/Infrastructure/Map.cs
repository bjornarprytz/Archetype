using System.Collections.Generic;
using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Proto;
using Archetype.View;
using Archetype.View.Atoms.Zones;
using Archetype.View.Infrastructure;

namespace Archetype.Game.Payloads.Infrastructure
{
    [Target("Map")]
    public interface IMap : IMapFront
    {
        new IEnumerable<IMapNode> Nodes { get; } 

        void Generate(IMapProtoData protoData);
    }

    internal class Map : IMap
    {
        private readonly IInstanceFactory _instanceFactory;
        private readonly List<IMapNode> _nodes = new();

        public Map(IInstanceFactory instanceFactory)
        {
            _instanceFactory = instanceFactory;
        }
        
        public IEnumerable<IMapNode> Nodes => _nodes;
        public void Generate(IMapProtoData protoData) // TODO: Solve this in another way. Probably using the instanceFactory etc. REMEMBER TO CONNECT THE NODES :) 
        {
            
            
            _nodes.AddRange(protoData.Nodes);
        }

        IEnumerable<IMapNodeFront> IMapFront.Nodes => Nodes;
    }
}
