using System;
using System.Linq.Expressions;
using Archetype.Game.Extensions;
using Archetype.View.Infrastructure;

namespace Archetype.Game.Payloads.Context.Effect.Base
{
    public interface IEffect
    {
        IEffectDescriptor CreateDescription();

        IEffectResult ResolveContext(IContext context);
    }

    public class Effect : IEffect
    {
        public Effect() { }
        
        private Func<IContext, IEffectResult> _resolveBacking;
        private Expression<Func<IContext, IEffectResult>> _resolveExpression;
        
        private Func<IContext, IEffectResult> Resolve
        {
            get
            {
                _resolveBacking ??= ResolveExpression.Compile();
                return _resolveBacking;
            }
        }
        
        public Expression<Func<IContext, IEffectResult>> ResolveExpression
        {
            get => _resolveExpression;
            set
            {
                _resolveExpression = value;
                _resolveBacking = null; // Force re-compile
            }
        }

        public IEffectDescriptor CreateDescription()
        {
            return ResolveExpression.CreateDescriptor();
        }

        public IEffectResult ResolveContext(IContext context)
        {
            return Resolve(context);
        }
    }
}