using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Context.Effect
{
    public interface IEffectContext<out TTarget> : IEffectContext
        where TTarget : IGameAtom
    {
        [Target("Target")]
        TTarget Target { get; }
    }

    public interface IEffectContext : IResolutionContext
    {
        IInstanceFactory InstanceFactory { get; }
        IGameAtom Source { get; }
    }
    
    public class EffectContext<TTarget> : EffectContext, IEffectContext<TTarget> 
        where TTarget : IGameAtom
    {
        public EffectContext(ICardContext cardContext, TTarget target) : base(cardContext)
        {
            Target = target;
        }
        public TTarget Target { get; }
    }
    
    public class EffectContext : IEffectContext
    {
        private readonly ICardContext _cardContext;

        public EffectContext(ICardContext cardContext)
        {
            _cardContext = cardContext;
        }

        public IGameState GameState => _cardContext.GameState;
        public IResolution PartialResults => _cardContext.PartialResults;
        
        public IInstanceFactory InstanceFactory => _cardContext.InstanceFactory;
        public IGameAtom Source => _cardContext.PlayArgs?.Player;
    }
}