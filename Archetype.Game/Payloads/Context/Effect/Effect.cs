using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Context.Effect
{
    public interface IEffect
    {
        int TargetIndex { get; }
        
        IEffectResult ResolveContext(ICardResolutionContext context);

        string ContextSensitiveRulesText(ICardResolutionContext cardResolutionContext);
        string PrintedRulesText();
    }
    
    public class Effect<TTarget> : IEffect 
        where TTarget : IGameAtom
    {
        private Func<IEffectResolutionContext<TTarget>, IEffectResult> _resolveBacking;
        private Expression<Func<IEffectResolutionContext<TTarget>, IEffectResult>> _resolveExpression;

        [JsonIgnore]
        private Func<IEffectResolutionContext<TTarget>, IEffectResult> Resolve
        {
            get
            {
                _resolveBacking ??= ResolveExpression.Compile();
                return _resolveBacking;
            }
        }

        [JsonIgnore]
        public Expression<Func<IEffectResolutionContext<TTarget>, IEffectResult>> ResolveExpression
        {
            get => _resolveExpression;
            set
            {
                _resolveExpression = value;
                _resolveBacking = null; // Force re-compile
            }
        }

        public int TargetIndex { get; set; }
        public IEffectResult ResolveContext(ICardResolutionContext context)
        {
            dynamic target = context.Targets.ElementAt(TargetIndex);
            
            return Resolve(new EffectResolutionContext<TTarget>(context, target));
        }

        public string ContextSensitiveRulesText(ICardResolutionContext cardResolutionContext)
        {
            return ResolveExpression.ContextSensitiveRulesText(new EffectResolutionContext<TTarget>(cardResolutionContext, default));
        }

        public string PrintedRulesText()
        {
            return ResolveExpression.PrintedRulesText();
        }
    }
    
    public class Effect : IEffect
    {
        private Func<IEffectResolutionContext, IEffectResult> _resolveBacking;
        private Expression<Func<IEffectResolutionContext, IEffectResult>> _resolveExpression;

        [JsonIgnore]
        private Func<IEffectResolutionContext, IEffectResult> Resolve
        {
            get
            {
                _resolveBacking ??= ResolveExpression.Compile();
                return _resolveBacking;
            }
        }

        [JsonIgnore]
        public Expression<Func<IEffectResolutionContext, IEffectResult>> ResolveExpression
        {
            get => _resolveExpression;
            set
            {
                _resolveExpression = value;
                _resolveBacking = null; // Force re-compile
            }
        }


        public int TargetIndex => -1;

        public IEffectResult ResolveContext(ICardResolutionContext context) => Resolve(new EffectResolutionContext(context));

        public string ContextSensitiveRulesText(ICardResolutionContext cardResolutionContext)
        {
            return ResolveExpression.ContextSensitiveRulesText(new EffectResolutionContext(cardResolutionContext));
        }

        public string PrintedRulesText()
        {
            return ResolveExpression.PrintedRulesText();
        }
    }
}
