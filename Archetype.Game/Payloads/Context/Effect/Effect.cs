using System;
using System.Linq.Expressions;
using Archetype.Game.Attributes;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.View.Infrastructure;

namespace Archetype.Game.Payloads.Context.Effect.Base
{
    public interface IEffect
    {
        IEffectDescriptor CreateDescription();

        IResult ResolveContext(IContext context);
    }

    public class Effect : IEffect
    {
        public Effect() { }
        
        private Func<IContext, IResult> _resolveBacking;
        private Expression<Func<IContext, IResult>> _resolveExpression;
        
        private Func<IContext, IResult> Resolve
        {
            get
            {
                _resolveBacking ??= ResolveExpression.Compile();
                return _resolveBacking;
            }
        }
        
        public Expression<Func<IContext, IResult>> ResolveExpression
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

        public IResult ResolveContext(IContext context)
        {
            return Resolve(context);
        }
    }
}