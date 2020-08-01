using System;

namespace Archetype
{
    public static class TargetDataMapperExtensions
    {
        public static Zone<Unit> GetZone(this UnitZone unitZone, GameState gameState)
        {
            return unitZone switch
            {
                UnitZone.Battlefield    => gameState.Battlefield,
                UnitZone.Graveyard      => gameState.Graveyard,
                _                       => throw new Exception($"Unrecognized UnitZone: {unitZone}"),
            };
        }

        public static Zone<Card> GetZone(this CardZone cardZone, Unit owner)
        {
            return cardZone switch
            {
                CardZone.Deck           => owner.Deck,
                CardZone.DiscardPile    => owner.DiscardPile,
                CardZone.Hand           => owner.Hand,
                CardZone.Pool           => owner.CardPool,
                _                       => throw new Exception($"Unrecognized CardZone: {cardZone}"),
            };
        }

        public static Func<Unit, bool> Getter(this TargetRelation relation, Unit source)
        {
            return relation switch
            {
                TargetRelation.Ally     => source.IsAllyOf,
                TargetRelation.Enemy    => source.IsEnemyOf,
                TargetRelation.Any      => (_) => true,
                _                       => throw new Exception($"Unrecognized TargetRelation: {relation}"),
            };
        }

        public static Func<Unit, bool> Getter(this Selfness selfness, Unit source)
        {
            return selfness switch
            {
                Selfness.Any    => (_) => true,
                Selfness.Other  => source.IsOther,
                Selfness.Me     => source.IsMe,
                _               => throw new Exception($"Unrecognized Sameness: {selfness}"),
            };
        }
    }
}
