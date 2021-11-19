using System;
using System.Linq;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;
using Newtonsoft.Json;

namespace Archetype.Game.Payloads.PlayContext
{
    public interface IEffect
    {
        int TargetIndex { get; }
        
        public object ResolveContext(ICardResolutionContext context);
        public string CreateRulesText(IGameState gameState);
        public string CreateRulesText();
    }
    
    public class Effect<TTarget, TResult> : IEffect 
        where TTarget : IGameAtom
    {
        private Func<IGameState, string> _rulesText;

        [JsonIgnore] public Func<IEffectResolutionContext<TTarget>, TResult> Resolve { get; set; }

        [JsonIgnore]
        public Func<IGameState, string> RulesText
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

        public string CreateRulesText(IGameState gameState)
        {
            return RulesText(gameState);
        }

        public string CreateRulesText()
        {
            return RulesText(null);
        }
    }
    
    public class Effect<TResult> : IEffect
    {
        private Func<IGameState, string> _rulesText;

        [JsonIgnore] public Func<IEffectResolutionContext, TResult> Resolve { get; set; }

        [JsonIgnore]
        public Func<IGameState, string> RulesText
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
        
        public string CreateRulesText(IGameState gameState)
        {
            return RulesText(gameState);
        }

        public string CreateRulesText()
        {
            return RulesText(null);
        }
    }
}
