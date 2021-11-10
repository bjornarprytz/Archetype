using System.Collections.Generic;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.Game.Payloads.Infrastructure
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