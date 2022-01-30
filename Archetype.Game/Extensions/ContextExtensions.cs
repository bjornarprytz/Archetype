using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Attributes;
using Archetype.Game.Factory;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Context;
using Archetype.View.Infrastructure;

namespace Archetype.Game.Extensions
{
    public static class ContextExtensions
    {
        public static IEffectResult TargetEach<TTarget, TResult>(this IEnumerable<TTarget> source, Func<TTarget, IEffectResult<TResult>> func)
            where TTarget : IGameAtom
        {
            return ResultFactory.CreateAggregate(source.Select(func).ToList());
        }
        
        [PropertyShortHand("GameState.Map.EachUnit")]
        public static IEnumerable<IUnit> EachUnit(this IContext context)
        {
            return context.GameState.Map.EachUnit();
        }
        
        public static T Target<T>(this IContext context)
            where T : IGameAtom
        {
            return context.TargetProvider.GetTarget<T>();
        }
        
        public static T Target<T>(this IContext context, int index)
            where T : IGameAtom
        {
            return context.TargetProvider.GetTarget<T>(index);
        }

        [ContextFact("equal to the total damage dealt")]
        public static int DamageDealt<T>(this T context)
            where T : IContext
        {
            return context.History
                .Entries.SelectMany(entry => entry.Result.Results)
                .Where(result => result.Verb is nameof(IUnit.Attack))
                .Select(r => (int) r.Result) // TODO: Refactor Results to not have to rely on reflection so much
                .Sum();
        }
    }
}