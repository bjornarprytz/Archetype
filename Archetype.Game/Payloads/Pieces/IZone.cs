using System.Collections.Generic;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IZone : IGamePiece
    {
        IEnumerable<ICard> Cards { get; }
    }
}