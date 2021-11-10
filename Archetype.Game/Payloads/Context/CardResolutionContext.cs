using System.Collections.Generic;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.Game.Payloads.Context
{
    public interface ICardResolutionContext
    {
        IGamePiece Source { get; }
        IEnumerable<IGamePiece> Targets { get; }
        
        IGameState GameState { get; }
    }
    
    public class CardResolutionContext : ICardResolutionContext
    {
        public CardResolutionContext(IGameState gameState, IGamePiece source, IEnumerable<IGamePiece> targets)
        {
            GameState = gameState;
            Source = source;
            Targets = targets;
        }
        
        public IGameState GameState { get; }
        public IGamePiece Source { get; }
        public IEnumerable<IGamePiece> Targets { get; }
    }
}