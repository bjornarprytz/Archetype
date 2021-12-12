using System;
using System.Linq.Expressions;
using Archetype.Builder.Exceptions;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Context.Effect;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Builder.Builders
{
    public interface ICardEffectBuilder : IBuilder<IEffect<ICardContext>>
    {
        public ICardEffectBuilder Resolve(Expression<Func<IEffectContext, IEffectResult>> expression);
    }

    public interface ICardEffectBuilder<TTarget> : IBuilder<IEffect<ICardContext>>
        where TTarget : IGameAtom
    {
        ICardEffectBuilder<TTarget> TargetIndex(int i);
        ICardEffectBuilder<TTarget> Resolve(Expression<Func<IEffectContext<TTarget>, IEffectResult>> expression);
    }

    public class CardEffectBuilder<TTarget> : ICardEffectBuilder<TTarget>
        where TTarget : IGameAtom
    {
        private readonly Effect<TTarget> _effect;

        public CardEffectBuilder()
        {
            _effect = new Effect<TTarget>();
        }
        
        public ICardEffectBuilder<TTarget> TargetIndex(int i)
        {
            _effect.TargetIndex = i;

            return this;
        }
        
        public ICardEffectBuilder<TTarget> Resolve(Expression<Func<IEffectContext<TTarget>, IEffectResult>> expression)
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

    public class CardEffectBuilder : ICardEffectBuilder
    {
        private readonly Effect _effect;
        
        public CardEffectBuilder()
        {
            _effect = new Effect();
        }
        
        public ICardEffectBuilder Resolve(Expression<Func<IEffectContext, IEffectResult>> expression)
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