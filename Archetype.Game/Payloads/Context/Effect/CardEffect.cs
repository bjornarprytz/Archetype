using System.Linq;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Context.Effect.Base;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Context.Effect
{
    internal class CardEffect<TTarget> : Effect<IEffectContext<TTarget>, IResult, ICardContext>
        where TTarget : IGameAtom
    {
        public int TargetIndex { get; set; }
        
        protected override IEffectContext<TTarget> DeriveContext(ICardContext parentContext)
        {
            dynamic target = parentContext.PlayArgs.Targets.ElementAt(TargetIndex);

            return new CardEffectContext(parentContext, target);
        }
        
        private record CardEffectContext : Context, IEffectContext<TTarget>
        {
            public CardEffectContext(ICardContext cardContext, TTarget target)
                : base(
                    cardContext.GameState,
                    cardContext.PartialResults,
                    cardContext.Owner)
            {
                Target = target;
            }

            public TTarget Target { get; }
        }
    }
    
    internal class CardEffect : Effect<IContext, IResult, ICardContext>
    {
        protected override IContext DeriveContext(ICardContext parentContext)
        {
            return new CardEffectContext(parentContext);
        }

        private record CardEffectContext : Context
        {
            public CardEffectContext(ICardContext cardContext)
                : base(
                    cardContext.GameState,
                    cardContext.PartialResults,
                    cardContext.Owner) { }
        }
    }
}
