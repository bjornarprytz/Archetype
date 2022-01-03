using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Infrastructure;

namespace Archetype.Game.Payloads.Context.Card
{
    public interface ITargetValidationContext<out TTarget>
        where TTarget : IGameAtom
    {
        TTarget Target { get; }
        IGameState GameState { get; }
    }

    public interface ITargetValidationContext
    {
        IGameAtom Target { get; }
        IGameState GameState { get; }
    }

    internal class TargetValidationContext<TTarget> : ITargetValidationContext<TTarget> 
        where TTarget : IGameAtom
    {
        public TargetValidationContext(IGameState gameState, TTarget target)
        {
            GameState = gameState;
            Target = target;
        }
        
        public TTarget Target { get; }
        public IGameState GameState { get; }
    }
    
    internal class TargetValidationContext : ITargetValidationContext
    {
        public TargetValidationContext(IGameState gameState, IGameAtom target)
        {
            GameState = gameState;
            Target = target;
        }
        
        public IGameAtom Target { get; }
        public IGameState GameState { get; }
    }
}