using System.Collections;
using System.Collections.Generic;

namespace Archetype.Core
{
    public interface IZone : IGamePiece
    {
        IEnumerable<ICard> Cards { get; }
    }
}