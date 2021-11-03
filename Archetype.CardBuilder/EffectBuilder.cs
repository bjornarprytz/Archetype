using System;
using Archetype.Game.Payloads.Metadata;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.CardBuilder
{
    public class EffectBuilder<TTarget, TResult> : IBuilder<IEffect>
        where TTarget : IGamePiece
    {
        private readonly Effect<TTarget, TResult> _effect;

        public EffectBuilder()
        {
            _effect = new Effect<TTarget, TResult>();
        }
        
        public EffectBuilder<TTarget, TResult> TargetIndex(int i)
        {
            _effect.TargetIndex = i;

            return this;
        }
        
        public EffectBuilder<TTarget, TResult> Resolve(Func<IEffectResolutionContext<TTarget>, TResult> func)
        {
            _effect.Resolve = func;

            return this;
        }
        
        public EffectBuilder<TTarget, TResult> Text(Func<IEffectResolutionContext<TTarget>, string> func)
        {
            _effect.RulesText = func;

            return this;
        }
        
        public EffectBuilder<TTarget, TResult> Text(string text)
        {
            _effect.RulesText = _ => text;

            return this;
        }

        public IEffect Build()
        {
            if (_effect.Resolve == null) throw new MissingResolutionFunctionException();

            return _effect;
        }
    }

    public class EffectBuilder<TResult> : IBuilder<IEffect>
    {
        private Effect<TResult> _effect;
        
        public EffectBuilder()
        {
            _effect = new Effect<TResult>();
        }
        
        public EffectBuilder<TResult> Resolve(Func<IEffectResolutionContext, TResult> func)
        {
            _effect.Resolve = func;

            return this;
        }
        
        public EffectBuilder<TResult> Text(Func<IEffectResolutionContext, string> func)
        {
            _effect.RulesText = func;

            return this;
        }
        
        public EffectBuilder<TResult> Text(string text)
        {
            _effect.RulesText = _ => text;

            return this;
        }

        public IEffect Build()
        {
            if (_effect.Resolve == null) throw new MissingResolutionFunctionException();

            return _effect;
        }
    }
}