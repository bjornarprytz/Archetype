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
        public static IEffectResult TargetEach<T>(this IEnumerable<T> source, Func<T, IEffectResult> func)
            where T : IGameAtom
        {
            return new AggregatedEffectResult(source.Select(func).ToList());
        } 
        
        [Group("Each Unit")]
        public static IEnumerable<IUnit> EachUnit<T>(this T context)
            where T : IEffectResolutionContext
        {
            return context.GameState.Map.Nodes.SelectMany(n => n.Contents);
        }

        [Group("Each unit in target zone")]
        public static IEnumerable<IUnit> UnitsInTargetZone<T>(this T context)
            where T : IEffectResolutionContext<IZone<IUnit>>
        {
            return context.Target.Contents;
        }
        
        [Group("Each card in the player's hand")]
        public static IEnumerable<ICard> CardsInPlayersHand<T>(this T context)
            where T : IEffectResolutionContext
        {
            return context.GameState.Player.Hand.Contents;
        }

        [Group("Each card in target unit's deck")]
        public static IEnumerable<ICard> CardsInTargetUnitsDeck<T>(this T context)
            where T : IEffectResolutionContext<IUnit>
        {
            return context.Target.Deck.Contents;
        }

        [ContextFact("Equal to the damage dealt by this card")]
        public static int DamageDealt<T>(this T context)
            where T : IEffectResolutionContext
        {
            return context.CardResolutionContext.PartialResults.VerbTotal[nameof(IUnit.Attack)];
        }
    }
}