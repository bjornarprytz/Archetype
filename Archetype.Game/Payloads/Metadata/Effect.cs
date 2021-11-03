using System;
using System.Linq;
using Archetype.Game.Payloads.Pieces;
using Newtonsoft.Json;

namespace Archetype.Game.Payloads.Metadata
{
    public class Effect<TTarget, TResult> : IEffect 
        where TTarget : IGamePiece
    {
        
        private Func<IEffectResolutionContext<TTarget>, string> _rulesText;

        public Effect() { }
        
        public Effect(
            int targetIndex,
            Func<IEffectResolutionContext<TTarget>, TResult> effectFunc,
            Func<IEffectResolutionContext<TTarget>, string> rulesTextFunc = null
            )
        {
            rulesTextFunc ??= _ => string.Empty;
            
            Resolve = effectFunc;
            RulesText = rulesTextFunc;
            TargetIndex = targetIndex;
        }

        [JsonIgnore] public Func<IEffectResolutionContext<TTarget>, TResult> Resolve { get; set; }

        [JsonIgnore]
        public Func<IEffectResolutionContext<TTarget>, string> RulesText
        {
            get
            {
                _rulesText ??= _ => string.Empty;
                return _rulesText;
            }
            set => _rulesText = value;
        }

        public int TargetIndex { get; set; }
        public object ResolveContext(ICardResolutionContext context)
        {
            dynamic target = context.Targets.ElementAt(TargetIndex);
            
            return Resolve(new EffectResolutionContext<TTarget>(context, target));
        }

        public string CallTextMethod(ICardResolutionContext context)
        {
            dynamic target = context.Targets.ElementAt(TargetIndex);

            return RulesText(new EffectResolutionContext<TTarget>(context, target));
        }
    }
    
    public class Effect<TResult> : IEffect
    {
        private Func<IEffectResolutionContext, string> _rulesText;

        public Effect() { }
        
        public Effect(
            Func<IEffectResolutionContext, TResult> effectFunc,
            Func<IEffectResolutionContext, string> rulesTextFunc = null
        )
        {
            rulesTextFunc ??= _ => string.Empty;
            
            Resolve = effectFunc;
            RulesText = rulesTextFunc;
        }

        [JsonIgnore] public Func<IEffectResolutionContext, TResult> Resolve { get; set; }

        [JsonIgnore]
        public Func<IEffectResolutionContext, string> RulesText
        {
            get
            {
                _rulesText ??= _ => string.Empty;
                return _rulesText;
            }
            set => _rulesText = value;
        }

        public int TargetIndex => -1;

        public object ResolveContext(ICardResolutionContext context)
        {
            return Resolve(new EffectResolutionContext(context));
        }
        
        public string CallTextMethod(ICardResolutionContext context)
        {
            return RulesText(new EffectResolutionContext(context));
        }
    }
}
