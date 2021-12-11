using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Context.Effect
{
    public interface IEffectResolutionContext<out TTarget> : IEffectResolutionContext
        where TTarget : IGameAtom
    {
        TTarget Target { get; }
    }

    public interface IEffectResolutionContext : IResolutionContext
    {
    }
    
    public class EffectResolutionContext<TTarget> : EffectResolutionContext, IEffectResolutionContext<TTarget> 
        where TTarget : IGameAtom
    {
        public EffectResolutionContext(ICardContext cardContext, TTarget target) : base(cardContext)
        {
            Target = target;
        }
        public TTarget Target { get; }
    }
    
    public class EffectResolutionContext : IEffectResolutionContext
    {
        private readonly ICardContext _cardContext;

        public EffectResolutionContext(ICardContext cardContext)
        {
            _cardContext = cardContext;
        }

        public IGameState GameState => _cardContext.GameState;
        public IResolution PartialResults => _cardContext.PartialResults;
        public void Dispose() { }
    }
}