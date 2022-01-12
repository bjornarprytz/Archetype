using System;
using System.Linq.Expressions;
using Archetype.Game.Attributes;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Infrastructure;

namespace Archetype.Game.Payloads.Context.Effect.Base
{
    public interface IEffect
    {
        string PrintedRulesText();
        string ContextRulesText(IContext context);

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

        public string PrintedRulesText()
        {
            return ResolveExpression.PrintedRulesText();
        }

        public string ContextRulesText(IContext context)
        {
            return ResolveExpression.ContextSensitiveRulesText(context);
        }

        public IResult ResolveContext(IContext context)
        {
            return Resolve(context);
        }

        public string ContextSensitiveRulesText(IContext parentContext)
        {
            return ResolveExpression.ContextSensitiveRulesText(parentContext);
        }
    }
}