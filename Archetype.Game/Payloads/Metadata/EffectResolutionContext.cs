using Archetype.Game.Payloads.Pieces;

namespace Archetype.Game.Payloads.Metadata
{
    public class EffectResolutionContext<TTarget> : EffectResolutionContext, IEffectResolutionContext<TTarget> 
        where TTarget : IGamePiece
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