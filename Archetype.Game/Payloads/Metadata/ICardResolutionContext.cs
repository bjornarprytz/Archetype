using System.Collections.Generic;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.Game.Payloads.Metadata
{
    public interface ICardResolutionContext
    {
        IGamePiece Source { get; }
        IEnumerable<IGamePiece> Targets { get; }
        
        IGameState GameState { get; }
    }
}