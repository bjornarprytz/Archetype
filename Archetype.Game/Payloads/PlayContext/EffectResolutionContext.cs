using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.PlayContext
{
    public interface IEffectResolutionContext<out TTarget> : IEffectResolutionContext
        where TTarget : IGameAtom
    {
        TTarget Target { get; }
    }

    public interface IEffectResolutionContext
    {
        ICardResolutionContext CardResolutionContext { get; }

        IGameState GameState => CardResolutionContext.GameState;
    }
    
    public class EffectResolutionContext<TTarget> : EffectResolutionContext, IEffectResolutionContext<TTarget> 
        where TTarget : IGameAtom
    {
        public EffectResolutionContext(ICardResolutionContext cardResolutionContext, TTarget target) : base(cardResolutionContext)
        {
            Target = target;
        }
        public TTarget Target { get; }
    }
    
    public class EffectResolutionContext : IEffectResolutionContext
    {
        public EffectResolutionContext(ICardResolutionContext cardResolutionContext)
        {
            CardResolutionContext = cardResolutionContext;
        }
        
        public ICardResolutionContext CardResolutionContext { get; }
    }
}