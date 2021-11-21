using System;
using System.Linq;
using System.Linq.Expressions;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces.Base;
using Newtonsoft.Json;

namespace Archetype.Game.Payloads.PlayContext
{
    public interface IEffect
    {
        int TargetIndex { get; }
        
        void ResolveContext(ICardResolutionContext context);

        string ContextSensitiveRulesText(IGameState gameState);
        string PrintedRulesText();
    }
    
    public class Effect<TTarget> : IEffect 
        where TTarget : IGameAtom
    {
        private Action<IEffectResolutionContext<TTarget>> _resolveBacking;
        private Expression<Action<IEffectResolutionContext<TTarget>>> _resolveExpression;

        [JsonIgnore]
        private Action<IEffectResolutionContext<TTarget>> Resolve
        {
            get
            {
                _resolveBacking ??= ResolveExpression.Compile();
                return _resolveBacking;
            }
        }

        [JsonIgnore]
        public Expression<Action<IEffectResolutionContext<TTarget>>> ResolveExpression
        {
            get => _resolveExpression;
            set
            {
                _resolveExpression = value;
                _resolveBacking = null; // Force re-compile
            }
        }

        public int TargetIndex { get; set; }
        public void ResolveContext(ICardResolutionContext context)
        {
            dynamic target = context.Targets.ElementAt(TargetIndex);
            
            Resolve(new EffectResolutionContext<TTarget>(context, target));
        }

        public string ContextSensitiveRulesText(IGameState gameState)
        {
            throw new NotImplementedException();
        }

        public string PrintedRulesText()
        {
            throw new NotImplementedException();
        }
    }
    
    public class Effect : IEffect
    {
        private Action<IEffectResolutionContext> _resolveBacking;
        private Expression<Action<IEffectResolutionContext>> _resolveExpression;

        [JsonIgnore]
        private Action<IEffectResolutionContext> Resolve
        {
            get
            {
                _resolveBacking ??= ResolveExpression.Compile();
                return _resolveBacking;
            }
        }

        [JsonIgnore]
        public Expression<Action<IEffectResolutionContext>> ResolveExpression
        {
            get => _resolveExpression;
            set
            {
                _resolveExpression = value;
                _resolveBacking = null; // Force re-compile
            }
        }


        public int TargetIndex => -1;

        public void ResolveContext(ICardResolutionContext context) => Resolve(new EffectResolutionContext(context));

        public string ContextSensitiveRulesText(IGameState gameState)
        {
            return ResolveExpression.ContextSensitiveRulesText(gameState);
        }

        public string PrintedRulesText()
        {
            return ResolveExpression.PrintedRulesText();
        }
    }
}
