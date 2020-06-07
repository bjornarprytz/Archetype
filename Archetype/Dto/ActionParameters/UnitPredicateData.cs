
using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class UnitPredicateData : TargetPredicateData
    {
        public UnitZone UnitZone { get; set; }
        public TargetRelation Relation { get; set; }

        public override IEnumerable<ITarget> GetOptions(Unit source, GameState gameState)
        {
            IEnumerable<Unit> zone;

            switch (UnitZone)
            {
                case UnitZone.Battlefield:
                    zone = gameState.Battlefield;
                    break;
                case UnitZone.Graveyard:
                    zone = gameState.Graveyard;
                    break;
                default:
                    throw new Exception($"Unrecognized UnitZone: {UnitZone}");
            }

            Func<Unit, bool> unitRestriction;

            switch (Relation)
            {
                case TargetRelation.Ally:
                    unitRestriction = source.AllyOf;
                    break;
                case TargetRelation.Enemy:
                    unitRestriction = source.EnemyOf;
                    break;
                case TargetRelation.Any:
                    unitRestriction = (_) => true;
                    break;
                default:
                    throw new Exception($"Unrecognized TargetRelation: {Relation}");
            }

            return zone.Where(unitRestriction);
        }
    }
}
