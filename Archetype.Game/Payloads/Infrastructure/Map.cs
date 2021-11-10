using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.Game.Payloads.Infrastructure
{
    public interface IMap
    {
        IEnumerable<IMapNode> Zones { get; }
    }

    public class Map : IMap
    {
        private readonly List<IMapNode> _zones;

        public Map(IEnumerable<IMapNode> zones)
        {
            _zones = zones.ToList();
        }


        public IEnumerable<IMapNode> Zones => _zones;
    }
}
