using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;
using Archetype.Game.Payloads.PlayContext;

namespace Archetype.Game.Extensions
{
    public static class ContextExtensions
    {
        public static IEffectResult TargetEach<TTarget, TResult>(this IEnumerable<TTarget> source, Func<TTarget, IEffectResult<TResult>> func)
            where TTarget : IGameAtom
        {
            return new AggregatedEffectResult<TResult>(source.Select(func).ToList());
        } 
        
        [Group("each unit")]
        public static IEnumerable<IUnit> EachUnit<T>(this T context)
            where T : IEffectResolutionContext
        {
            return context.GameState.Map.Nodes.SelectMany(n => n.Contents);
        }

        [Group("each unit in target zone")]
        public static IEnumerable<IUnit> UnitsInTargetZone<T>(this T context)
            where T : IEffectResolutionContext<IZone<IUnit>>
        {
            return context.Target.Contents;
        }
        
        [Group("each card in the player's hand")]
        public static IEnumerable<ICard> CardsInPlayersHand<T>(this T context)
            where T : IEffectResolutionContext
        {
            return context.GameState.Player.Hand.Contents;
        }

        [ContextFact("equal to the damage dealt by this card")]
        public static int DamageDealt<T>(this T context)
            where T : IEffectResolutionContext
        {
            return context.CardResolutionContext.PartialResults
                .Results
                .Where(result => result.Verb is nameof(IUnit.Attack))
                .Select(r => (int) r.Result) // TODO: Refactor Results to not have to rely on reflection so much
                .Sum();
        }
    }
}