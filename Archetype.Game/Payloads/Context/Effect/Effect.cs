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
        string PrintedRulesText();
    }
    
    public interface IEffect<in TContext> : IEffect
        where TContext : IResolutionContext
    {
        IEffectResult ResolveContext(TContext context);
        string ContextSensitiveRulesText(TContext cardResolutionContext);
        
    }
    
    public class Effect<TTarget> : IEffect<ICardContext>
        where TTarget : IGameAtom
    {
        private Func<IEffectContext<TTarget>, IEffectResult> _resolveBacking;
        private Expression<Func<IEffectContext<TTarget>, IEffectResult>> _resolveExpression;

        [JsonIgnore]
        private Func<IEffectContext<TTarget>, IEffectResult> Resolve
        {
            get
            {
                _resolveBacking ??= ResolveExpression.Compile();
                return _resolveBacking;
            }
        }

        [JsonIgnore]
        public Expression<Func<IEffectContext<TTarget>, IEffectResult>> ResolveExpression
        {
            get => _resolveExpression;
            set
            {
                _resolveExpression = value;
                _resolveBacking = null; // Force re-compile
            }
        }
        
        public int TargetIndex { get; set; }
        public IEffectResult ResolveContext(ICardContext context)
        {
            dynamic target = context.PlayArgs.Targets.ElementAt(TargetIndex);
            
            return Resolve(new EffectContext<TTarget>(context, target));
        }

        public string ContextSensitiveRulesText(ICardContext cardContext)
        {
            return ResolveExpression.ContextSensitiveRulesText(new EffectContext<TTarget>(cardContext, default));
        }

        public string PrintedRulesText()
        {
            return ResolveExpression.PrintedRulesText();
        }
    }
    
    public class Effect : IEffect<ICardContext>
    {
        private Func<IEffectContext, IEffectResult> _resolveBacking;
        private Expression<Func<IEffectContext, IEffectResult>> _resolveExpression;

        [JsonIgnore]
        private Func<IEffectContext, IEffectResult> Resolve
        {
            get
            {
                _resolveBacking ??= ResolveExpression.Compile();
                return _resolveBacking;
            }
        }

        [JsonIgnore]
        public Expression<Func<IEffectContext, IEffectResult>> ResolveExpression
        {
            get => _resolveExpression;
            set
            {
                _resolveExpression = value;
                _resolveBacking = null; // Force re-compile
            }
        }

        public IEffectResult ResolveContext(ICardContext context) => Resolve(new EffectContext(context));

        public string ContextSensitiveRulesText(ICardContext cardContext)
        {
            return ResolveExpression.ContextSensitiveRulesText(new EffectContext(cardContext));
        }

        public string PrintedRulesText()
        {
            return ResolveExpression.PrintedRulesText();
        }
    }
}
