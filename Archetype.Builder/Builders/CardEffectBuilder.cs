using System;
using System.Linq.Expressions;
using Archetype.Builder.Builders.Base;
using Archetype.Builder.Exceptions;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Context.Effect;
using Archetype.Game.Payloads.Context.Effect.Base;

namespace Archetype.Builder.Builders
{
    public interface ICardEffectBuilder : IBuilder<IEffect<ICardContext>>
    {
        public ICardEffectBuilder Resolve(Expression<Func<IContext, IResult>> expression);
    }

    public interface ICardEffectBuilder<TTarget> : IBuilder<IEffect<ICardContext>>
        where TTarget : IGameAtom
    {
        ICardEffectBuilder<TTarget> TargetIndex(int i);
        ICardEffectBuilder<TTarget> Resolve(Expression<Func<IEffectContext<TTarget>, IResult>> expression);
    }

    internal class CardEffectBuilder<TTarget> : ICardEffectBuilder<TTarget>
        where TTarget : IGameAtom
    {
        private readonly CardEffect<TTarget> _cardEffect;

        public CardEffectBuilder()
        {
            _cardEffect = new CardEffect<TTarget>();
        }
        
        public ICardEffectBuilder<TTarget> TargetIndex(int i)
        {
            _cardEffect.TargetIndex = i;

            return this;
        }
        
        public ICardEffectBuilder<TTarget> Resolve(Expression<Func<IEffectContext<TTarget>, IResult>> expression)
        {
            _cardEffect.ResolveExpression = expression;

            return this;
        }

        public IEffect<ICardContext> Build()
        {
            if (_cardEffect.ResolveExpression == null) throw new MissingResolutionExpressionException();

            return _cardEffect;
        }
    }

    internal class CardEffectBuilder : ICardEffectBuilder
    {
        private readonly CardEffect _cardEffect;
        
        public CardEffectBuilder()
        {
            _cardEffect = new CardEffect();
        }
        
        public ICardEffectBuilder Resolve(Expression<Func<IContext, IResult>> expression)
        {
            _cardEffect.ResolveExpression = expression;

            return this;
        }

        public IEffect<ICardContext> Build()
        {
            if (_cardEffect.ResolveExpression == null) throw new MissingResolutionExpressionException();

            return _cardEffect;
        }
    }
}