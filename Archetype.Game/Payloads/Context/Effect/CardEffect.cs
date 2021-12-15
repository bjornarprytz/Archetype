using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Context.Effect.Base;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Context.Effect
{
    public class CardEffect<TTarget> : Effect<IEffectContext<TTarget>, IResult>, IEffect<ICardContext>
        where TTarget : IGameAtom
    {
        public int TargetIndex { get; set; }
        public IResult ResolveContext(ICardContext context) // TODO: Take Effect context here and caller handles context creation
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
    
    public class CardEffect : Effect<IContext, IResult>, IEffect<ICardContext>
    {
        public IResult ResolveContext(ICardContext context) => Resolve(new EffectContext(context));

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
