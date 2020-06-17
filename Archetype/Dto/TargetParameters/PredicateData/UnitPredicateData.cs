
using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class UnitPredicateData : TargetPredicateData
    {
        public UnitZone UnitZone { get; set; }
        public TargetRelation Relation { get; set; }
        public Selfness Selfness { get; set; }

        public override IEnumerable<ITarget> GetOptions(Unit source, GameState gameState)
        {
            return UnitZone.GetZone(gameState)
                .Where(Selfness.Getter(source))
                .Where(Relation.Getter(source));
        }
    }
}
