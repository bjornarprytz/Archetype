using System.Collections.Generic;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IBoard
    {
        IEnumerable<IZone> Zones { get; }
    }
}
