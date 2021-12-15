using System;
using System.Linq.Expressions;

namespace Archetype.Game.Payloads.Context.Effect.Base
{
    public interface IEffect
    {
        string PrintedRulesText();
    }
    
    public interface IEffect<in TContext> : IEffect
        where TContext : IContext
    {
        IResult ResolveContext(TContext context);
        string ContextSensitiveRulesText(TContext cardResolutionContext);
        
    }

    public abstract class Effect<TContext, TResult>
        where TContext : IContext
        where TResult : IResult
    {
        protected Effect() { }
        
        private Func<TContext, TResult> _resolveBacking;
        private Expression<Func<TContext, TResult>> _resolveExpression;
        
        protected Func<TContext, TResult> Resolve
        {
            get
            {
                _resolveBacking ??= ResolveExpression.Compile();
                return _resolveBacking;
            }
        }
        
        public Expression<Func<TContext, TResult>> ResolveExpression
        {
            get => _resolveExpression;
            set
            {
                _resolveExpression = value;
                _resolveBacking = null; // Force re-compile
            }
        }
    }
}