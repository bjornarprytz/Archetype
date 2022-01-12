using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Context.Effect;
using Archetype.Game.Payloads.Context.Effect.Base;
using Archetype.View.Atoms;

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
        public static IEnumerable<IUnit> EachUnit(this IContext context)
        {
            return context.GameState.Map.EachUnit();
        }

        [Group("each unit in target zone")]
        public static IEnumerable<IUnit> UnitsInTargetZone(this IContext context)
        {
            return context.Target<IZone<IUnit>>().Contents;
        }
        
        [Group("each card in the player's hand")]
        public static IEnumerable<ICard> CardsInPlayersHand(this IContext context)
        {
            return context.GameState.Player.Hand.Contents;
        }

        [ContextProperty("owner")]
        public static IGameAtom Owner(this IContext context)
        {
            return context.Source.Owner;
        }
        
        public static T Target<T>(this IContext context)
            where T : IGameAtom
        {
            return context.TargetProvider.GetTarget<T>();
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