using Archetype.Game.Payloads.Pieces;

namespace Archetype.Game.Payloads.Metadata
{
    public interface ITargetValidationContext<out TTarget>
        where TTarget : IGamePiece
    {
        TTarget Target { get; }
        IGameState GameState { get; }
    }
    
    public interface ITargetValidationContext
    {
        IGamePiece Target { get; }
        IGameState GameState { get; }
    }

    public class TargetValidationContext<TTarget> : ITargetValidationContext<TTarget> 
        where TTarget : IGamePiece
    {
        public TargetValidationContext(IGameState gameState, TTarget target)
        {
            GameState = gameState;
            Target = target;
        }
        
        public TTarget Target { get; }
        public IGameState GameState { get; }
    }
    
    public class TargetValidationContext : ITargetValidationContext
    {
        public TargetValidationContext(IGameState gameState, IGamePiece target)
        {
            GameState = gameState;
            Target = target;
        }
        
        public IGamePiece Target { get; }
        public IGameState GameState { get; }
    }
}