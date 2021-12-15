using System;
using System.Linq.Expressions;
using Archetype.Game.Payloads.Context.Effect.Base;
using Archetype.Game.Payloads.Context.Trigger;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Context.Effect
{
    public class TriggerEffect<TSource> : Effect<ITriggerContext<TSource>, IResult>, IEffect<ITriggerContext<TSource>> 
        where TSource : IGameAtom
    {
        
        public string PrintedRulesText()
        {
            throw new System.NotImplementedException();
        }

        public IResult ResolveContext(ITriggerContext<TSource> context)
        {
            throw new System.NotImplementedException();
        }

        public string ContextSensitiveRulesText(ITriggerContext<TSource> cardResolutionContext)
        {
            throw new System.NotImplementedException();
        }
    }
}