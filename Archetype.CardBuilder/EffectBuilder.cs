using System;
using System.Collections;
using System.Collections.Generic;
using Archetype.Core;

namespace Archetype.CardBuilder
{
    public class EffectBuilder<TTarget, TResult> : BaseBuilder<IEffectMetaData>
        where TTarget : IGamePiece
    {
        private EffectData<TTarget, TResult> _effect => ( EffectData<TTarget, TResult>) Construction;

        public EffectBuilder(EffectData<TTarget, TResult> template = null) : base(() => template ?? new EffectData<TTarget, TResult>()) { }
        
        public EffectBuilder<TTarget, TResult> Validate(Func<TTarget, IGameState, bool> func)
        {
            _effect.Validate = func;

            return this;
        }
        
        public EffectBuilder<TTarget, TResult> Resolve(Func<TTarget, IGameState, TResult> func)
        {
            _effect.Resolve = func;

            return this;
        }
        
        public EffectBuilder<TTarget, TResult> Text(Func<TTarget, IGameState, string> func)
        {
            _effect.RulesText = func;

            return this;
        }
        
        public EffectBuilder<TTarget, TResult> Text(string text)
        {
            _effect.RulesText = (piece, state) => text;

            return this;
        }
        
        protected override void PreBuild()
        {
            if (_effect.Resolve == null) throw new Exception("Card must have an effect");
        }
    }
}