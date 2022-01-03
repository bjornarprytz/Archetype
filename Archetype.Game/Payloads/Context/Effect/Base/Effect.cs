using System;
using System.Linq.Expressions;
using Archetype.Game.Attributes;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Infrastructure;

namespace Archetype.Game.Payloads.Context.Effect.Base
{
    public interface IEffectContext<out TTarget> : IContext
        where TTarget : IGameAtom
    {
        [Target("Target")]
        TTarget Target { get; }
    }

    public interface IEffect
    {
        string PrintedRulesText();
        string ContextRulesText(IContext context);
    }

    public interface IEffect<in TContext> : IEffect
        where TContext : IContext
    {
        IResult ResolveContext(TContext context);
    }

    public abstract class Effect<TContext, TResult, TParentContext> : IEffect<TParentContext>
        where TContext : IContext
        where TResult : IResult
        where TParentContext : IContext
    {
        protected Effect() { }
        
        private Func<TContext, TResult> _resolveBacking;
        private Expression<Func<TContext, TResult>> _resolveExpression;
        
        private Func<TContext, TResult> Resolve
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

        public string PrintedRulesText()
        {
            return ResolveExpression.PrintedRulesText();
        }

        public string ContextRulesText(IContext context)
        {
            return ResolveExpression.ContextSensitiveRulesText((TContext)context); // TODO: Confirm this works
        }

        public IResult ResolveContext(TParentContext parentContext)
        {
            var context = DeriveContext(parentContext);

            return Resolve(context);
        }

        public string ContextSensitiveRulesText(TParentContext parentContext)
        {
            return ResolveExpression.ContextSensitiveRulesText(DeriveContext(parentContext));
        }

        protected abstract TContext DeriveContext(TParentContext parentContext);
        
        protected record Context(IGameState GameState, IResultsReader PartialResults, IGameAtom Owner) 
            : IContext;
    }
    
    internal abstract class Effect<TContext, TResult> : Effect<TContext, TResult, TContext>
        where TContext : IContext
        where TResult : IResult
    {
        protected override TContext DeriveContext(TContext parentContext) => parentContext;
    }
}