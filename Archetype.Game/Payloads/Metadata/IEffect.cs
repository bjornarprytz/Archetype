using System;
using System.Collections.Generic;
using Archetype.Core;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.Game.Payloads.Metadata
{
    public interface IEffect
    {
        int TargetIndex { get; }
        
        public object CallResolveMethod(IList<IGamePiece> availableTargets, IGameState gameState);
        public string CallTextMethod(IList<IGamePiece> gamePiece, IGameState gameState);
    }
}
