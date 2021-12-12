using System;
using System.Linq.Expressions;
using Archetype.Builder.Exceptions;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Context.Effect;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Builder.Builders
{
    public class CardEffectBuilder<TTarget> : IBuilder<IEffect<ICardContext>>
        where TTarget : IGameAtom
    {
        private readonly Effect<TTarget> _effect;

        internal CardEffectBuilder()
        {
            _effect = new Effect<TTarget>();
        }
        
        public CardEffectBuilder<TTarget> TargetIndex(int i)
        {
            _effect.TargetIndex = i;

            return this;
        }
        
        public CardEffectBuilder<TTarget> Resolve(Expression<Func<IEffectContext<TTarget>, IEffectResult>> expression)
        {
            _effect.ResolveExpression = expression;

            return this;
        }

        public IEffect<ICardContext> Build()
        {
            if (_effect.ResolveExpression == null) throw new MissingResolutionExpressionException();

            return _effect;
        }
    }

    public class CardEffectBuilder : IBuilder<IEffect<ICardContext>>
    {
        private readonly Effect _effect;
        
        public CardEffectBuilder()
        {
            _effect = new Effect();
        }
        
        public CardEffectBuilder Resolve(Expression<Func<IEffectContext, IEffectResult>> expression)
        {
            _effect.ResolveExpression = expression;

            return this;
        }

        public IEffect<ICardContext> Build()
        {
            if (_effect.ResolveExpression == null) throw new MissingResolutionExpressionException();

            return _effect;
        }
    }
}