
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Archetype
{
    public class UnitPredicateData : TargetPredicateData
    {
        public UnitZone UnitZone { get; set; }
        public TargetRelation Relation { get; set; }
        public Sameness Sameness { get; set; }

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

            

            Func<Unit, bool> factionRestriction;

            switch (Relation)
            {
                case TargetRelation.Ally:
                    factionRestriction = source.AllyOf;
                    break;
                case TargetRelation.Enemy:
                    factionRestriction = source.EnemyOf;
                    break;
                case TargetRelation.Any:
                    factionRestriction = (_) => true;
                    break;
                default:
                    throw new Exception($"Unrecognized TargetRelation: {Relation}");
            }

            Func<Unit, bool> whoRestriction;

            switch (Sameness)
            {
                case Sameness.Any:
                    whoRestriction = (_) => true;
                    break;
                case Sameness.Other:
                    whoRestriction = source.Other;
                    break;
                case Sameness.Me:
                    whoRestriction = source.Me;
                    break;
                default:
                    throw new Exception($"Unrecognized Sameness: {Sameness}");
            }

            return zone
                .Where(whoRestriction)
                .Where(factionRestriction);
        }
    }
}
