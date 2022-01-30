using System;
using System.Collections.Generic;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Context;
using Archetype.View.Infrastructure;

namespace Archetype.Game.Extensions
{
    internal static class MoveExtensions
    {
        public static IEnumerable<IResult> MoveAlong(this ICreature creature, IReadOnlyDictionary<IMapNode, IMapNode> path, IMapNode target)
        {
            var results = new List<IResult>();
            
            for (var i = 0; i < creature.Movement; i++)
            {
                if (creature.CurrentZone is not IMapNode node)
                    throw new InvalidOperationException($"Creature {creature} must be on the map");

                if (node == target)
                    break;

                if (node.CalculateDefenseAgainst(creature) > creature.Strength)
                    continue;

                results.Add(creature.MoveTo(path[node]));
            }

            return results;
        }
    }
}