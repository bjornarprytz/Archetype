using Archetype.Core.Atoms;
using Archetype.Core.Extensions;
using Archetype.Core.Play;

namespace Archetype.Engine.Extensions;

internal static class MoveExtensions
{
    public static IEnumerable<IEffectResult> MoveAlong(this ICreature creature, IReadOnlyDictionary<IMapNode, IMapNode> path, IMapNode target)
    {
        var results = new List<IEffectResult>();
            
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