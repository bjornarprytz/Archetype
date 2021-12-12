using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Extensions
{
    public static class CombatExtensions
    {
        public static IEnumerable<IEffectResult> ResolveCombat(this IMapNode node)
        {
            var enemies = node.Enemies<ICreature>();
            var friendlies = node.Friendlies<ICreature>();

            var enemyTargets = node.Enemies<IUnit>().ToList();
            var friendlyTargets = node.Friendlies<IUnit>().ToList();

            var results = new List<IEffectResult>();

            results.AddRange(
                enemies.GroupFight(friendlyTargets));
            results.AddRange(
                friendlies.GroupFight(enemyTargets));

            return results;
        }

        public static IEnumerable<IEffectResult> BuryTheDead(this IMapNode node)
        {
            var results = new List<IEffectResult>();
            
            foreach (var deadCreature in node.EachCreature().Where(creature => creature.IsDead()))
            {
                results.Add(deadCreature.MoveTo(node.Graveyard));
            }

            return results;
        }

        private static IEffectResult Fight(this ICreature creature, IUnit target)
        {
            return target.Attack(creature.Strength);
        }
        
        private static IUnit PickTarget(this IList<IUnit> potentialTargets)
        {
            var unitWithHighestDefense = potentialTargets.Where(target => target.Defense > 0)
                .OrderByDescending(target => target.Defense).FirstOrDefault();

            if (unitWithHighestDefense is not null)
            {
                return unitWithHighestDefense;
            }

            var unitWithLowestHealth = potentialTargets.OrderBy(target => target.Health).FirstOrDefault();

            return unitWithLowestHealth;
        }


        private static IEnumerable<IEffectResult> GroupFight(this IEnumerable<ICreature> attackers,
            IList<IUnit> potentialTargets)
        {
            var results = new List<IEffectResult>();
            
            foreach (var attacker in attackers)
            {
                if (potentialTargets.IsEmpty())
                    break;
                
                var target = potentialTargets.PickTarget();
                
                if (target is null)
                    continue;

                results.Add(attacker.Fight(target));
                
                if (target.IsDead())
                    potentialTargets.Remove(target);
            }

            return results;
        }
    }
}