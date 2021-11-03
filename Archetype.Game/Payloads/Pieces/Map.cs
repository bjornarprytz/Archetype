using System.Collections.Generic;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IMap
    {
        IEnumerable<IMapNode> Zones { get; }
    }

    public class Map : IMap
    {
        private readonly List<IMapNode> _zones;

        public Map(List<IMapNode> zones)
        {
            _zones = zones;
        }


        public IEnumerable<IMapNode> Zones => _zones;
    }
}
