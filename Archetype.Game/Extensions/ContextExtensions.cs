using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Context.Effect;
using Archetype.Game.Payloads.Context.Effect.Base;

namespace Archetype.Game.Extensions
{
    public static class ContextExtensions
    {
        public static IResult TargetEach<TTarget, TResult>(this IEnumerable<TTarget> source, Func<TTarget, IResult<TResult>> func)
            where TTarget : IGameAtom
        {
            return new AggregatedEffectResult<TResult>(source.Select(func).ToList());
        }
        
        [Group("each unit")]
        public static IEnumerable<IUnit> EachUnit<T>(this T context)
            where T : IContext
        {
            return context.GameState.Map.EachUnit();
        }

        [Group("each unit in target zone")]
        public static IEnumerable<IUnit> UnitsInTargetZone<T>(this T context)
            where T : IEffectContext<IZone<IUnit>>
        {
            return context.Target.Contents;
        }
        
        [Group("each card in the player's hand")]
        public static IEnumerable<ICard> CardsInPlayersHand<T>(this T context)
            where T : IContext
        {
            return context.GameState.Player.Hand.Contents;
        }

        [ContextFact("equal to the damage dealt by this card")]
        public static int DamageDealt<T>(this T context)
            where T : IContext
        {
            return context.PartialResults
                .Results
                .Where(result => result.Verb is nameof(IUnit.Attack))
                .Select(r => (int) r.Result) // TODO: Refactor Results to not have to rely on reflection so much
                .Sum();
        }
    }
}