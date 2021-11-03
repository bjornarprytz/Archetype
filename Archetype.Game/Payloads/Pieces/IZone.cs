using System;
using System.Collections.Generic;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IZone : IGamePiece
    {
    }

    public interface IZone<out T> : IZone
        where T : IGamePiece
    {
        IEnumerable<T> Contents { get; }
    }
}