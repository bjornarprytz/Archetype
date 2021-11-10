using System.Collections.Generic;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Builder
{
    public class MapBuilder : IBuilder<IMapProtoData>
    {
        private readonly List<IMapNode> _nodes = new();

        private readonly IMapProtoData _mapProtoData;
        
        public MapBuilder()
        {
            _mapProtoData = new MapProtoData(_nodes);
        }
        
        public IMapProtoData Build()
        {
            return _mapProtoData;
        }
    }
}