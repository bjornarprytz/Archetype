using System;
using System.Linq.Expressions;
using Archetype.Builder.Exceptions;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Context.Effect;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Builder
{
    public class EffectBuilder<TTarget> : IBuilder<IEffect>
        where TTarget : IGameAtom
    {
        private readonly Effect<TTarget> _effect;

        internal EffectBuilder()
        {
            _effect = new Effect<TTarget>();
        }
        
        public EffectBuilder<TTarget> TargetIndex(int i)
        {
            _effect.TargetIndex = i;

            return this;
        }
        
        public EffectBuilder<TTarget> Resolve(Expression<Func<IEffectResolutionContext<TTarget>, IEffectResult>> expression)
        {
            _effect.ResolveExpression = expression;

            return this;
        }

        public IEffect Build()
        {
            if (_effect.ResolveExpression == null) throw new MissingResolutionExpressionException();

            return _effect;
        }
    }

    public class EffectBuilder : IBuilder<IEffect>
    {
        private readonly Effect _effect;
        
        public EffectBuilder()
        {
            _effect = new Effect();
        }
        
        public EffectBuilder Resolve(Expression<Func<IEffectResolutionContext, IEffectResult>> expression)
        {
            _effect.ResolveExpression = expression;

            return this;
        }

        public IEffect Build()
        {
            if (_effect.ResolveExpression == null) throw new MissingResolutionExpressionException();

            return _effect;
        }
    }
}