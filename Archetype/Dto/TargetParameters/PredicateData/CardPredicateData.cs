using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class CardPredicateData : TargetPredicateData
    {
        public CardZone CardZone { get; set; }
        public UnitPredicateData OwnerPredicate { get; set; }

        public override IEnumerable<ITarget> GetOptions(Unit source, GameState gameState)
        {
            IEnumerable<Unit> candidateOwners = OwnerPredicate.GetOptions(source, gameState) as IEnumerable<Unit>;

            Func<Unit, IEnumerable<Card>> zoneChooser;

            switch (CardZone)
            {
                case CardZone.Deck:
                    zoneChooser = (owner) => owner.Deck;
                    break;
                case CardZone.DiscardPile:
                    zoneChooser = (owner) => owner.DiscardPile;
                    break;
                case CardZone.Hand:
                    zoneChooser = (owner) => owner.Hand;
                    break;
                case CardZone.Pool:
                    zoneChooser = (owner) => owner.CardPool;
                    break;
                default:
                    throw new Exception($"Unrecognized CardZone: {CardZone}");
            }

            return candidateOwners.SelectMany(zoneChooser);
        }
    }
}
