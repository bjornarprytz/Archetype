using System;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.Game.Payloads.Metadata
{
    public interface ITarget
    {
        Type TargetType { get; }
        bool CallTargetValidationMethod(IGamePiece gamePiece, IGameState gameState);
    }
}