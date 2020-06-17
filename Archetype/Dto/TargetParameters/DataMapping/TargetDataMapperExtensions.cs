using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public static class TargetDataMapperExtensions
    {
        public static IEnumerable<Unit> GetZone(this UnitZone unitZone, GameState gameState)
        {
            switch (unitZone)
            {
                case UnitZone.Battlefield:
                    return gameState.Battlefield;
                case UnitZone.Graveyard:
                    return gameState.Graveyard;
                default:
                    throw new Exception($"Unrecognized UnitZone: {unitZone}");
            }
        }

        public static IEnumerable<Card> GetZone(this CardZone cardZone, Unit owner)
        {
            switch (cardZone)
            {
                case CardZone.Deck:
                    return owner.Deck;
                case CardZone.DiscardPile:
                    return owner.DiscardPile;
                case CardZone.Hand:
                    return owner.Hand;
                case CardZone.Pool:
                    return owner.CardPool;
                default:
                    throw new Exception($"Unrecognized CardZone: {cardZone}");
            }
        }

        public static Func<Unit, bool> Getter(this TargetRelation relation, Unit source)
        {
            switch (relation)
            {
                case TargetRelation.Ally:
                    return source.IsAllyOf;
                case TargetRelation.Enemy:
                    return source.IsEnemyOf;
                case TargetRelation.Any:
                    return (_) => true;
                default:
                    throw new Exception($"Unrecognized TargetRelation: {relation}");
            }
        }

        public static Func<Unit, bool> Getter(this Selfness selfness, Unit source)
        {
            switch (selfness)
            {
                case Selfness.Any:
                    return (_) => true;
                case Selfness.Other:
                    return source.IsOther;
                case Selfness.Me:
                    return source.IsMe;
                default:
                    throw new Exception($"Unrecognized Sameness: {selfness}");
            }
        }
    }
}
