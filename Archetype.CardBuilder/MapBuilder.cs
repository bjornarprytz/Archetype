using System.Collections.Generic;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.CardBuilder
{
    public class MapBuilder : IBuilder<IMap>
    {
        private readonly List<IMapNode> _nodes = new();

        private readonly IMap _map;
        
        public MapBuilder()
        {
            _map = new Map(_nodes);
        }        
        
        public IMap Build()
        {
            return _map;
        }
    }
}