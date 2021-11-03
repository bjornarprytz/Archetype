using System.Collections.Generic;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IMap
    {
        IEnumerable<IMapNode> Zones { get; }
    }
}
