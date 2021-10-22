using System.Collections.Generic;

namespace Archetype.Core
{
    public interface IBoard
    {
        IEnumerable<IZone> Zones { get; }
    }
}
