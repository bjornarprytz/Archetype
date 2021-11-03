using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IZone : IGamePiece
    {
        IEnumerable<IGamePiece> GamePieces { get; }
    }

    public interface IZone<out T> : IZone
        where T : IGamePiece
    {
        IEnumerable<T> Contents => GamePieces.Where(p => p is T).Cast<T>();
    }
}