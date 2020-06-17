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
            var candidateOwners = OwnerPredicate.GetOptions(source, gameState) as IEnumerable<Unit>;

            return candidateOwners.SelectMany(unit => CardZone.GetZone(unit));
        }
    }
}
