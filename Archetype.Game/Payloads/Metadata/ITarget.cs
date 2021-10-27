using System;
using Archetype.Core;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.Game.Payloads.Metadata
{
    public interface ITarget
    {
        bool CallTargetValidationMethod(IGamePiece gamePiece, IGameState gameState);

        TargetData CreateReadOnlyData();
    }
}