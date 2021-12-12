using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Extensions
{
    public static class MapExtensions
    {

        public static IEnumerable<IUnit> EachUnit(this IMap map)
        {
            return map.Nodes.SelectMany(node => node.Contents);
        }
        public static IEnumerable<ICreature> EachEnemyCreature(this IMap map)
        {
            return map.Nodes.SelectMany(Enemies<ICreature>);
        }
        
        public static IEnumerable<ICreature> EachFriendlyCreature(this IMap map)
        {
            return map.Nodes.SelectMany(Friendlies<ICreature>);
        }

        public static IEnumerable<IStructure> EachFriendlyStructure(this IMap map)
        {
            return map.Nodes.SelectMany(Friendlies<IStructure>);
        }
        
        public static IEnumerable<IStructure> EachEnemyStructure(this IMap map)
        {
            return map.Nodes.SelectMany(Enemies<IStructure>);
        }

        public static IEnumerable<T> Friendlies<T>(this IMapNode node)
            where T : IUnit
        {
            return node.Contents.OfType<T>()
                .Where(c => c.IsFriendly());
        }
        
        public static IEnumerable<T> Enemies<T>(this IMapNode node)
            where T : IUnit
        {
            return node.Contents.OfType<T>()
                .Where(c => c.IsEnemy());
        }

        public static IEnumerable<T> EnemiesOf<T>(this IMapNode node, IGameAtom atom)
            where T : IUnit
        {
            var myTopOwner = atom.TopOwner();

            return node.Contents.OfType<T>().Where(t => t.TopOwner() != myTopOwner);
        }

        public static IEnumerable<ICreature> EachCreature(this IMapNode node)
        {
            return node.Contents.OfType<ICreature>();
        }
        
        private static bool IsContested(this IMapNode node)
        {
            return node.Contents.Any(c => c.IsEnemyOf(node));
        }

        public static int CalculateDefenseAgainst(this IMapNode node, IUnit attacker)
        {
            return node.EnemiesOf<IUnit>(attacker).Sum(defender => defender.Defense);
        }

        public static IReadOnlyDictionary<IMapNode, IMapNode> CalculateShortestPaths(this IMapNode targetNode)
        {
            var dict = new Dictionary<IMapNode, IMapNode> { { targetNode, targetNode } };

            UpdateNeighbours(targetNode);

            return dict;

            void UpdateNeighbours(IMapNode node)
            {
                var neighboursToUpdate = new List<IMapNode>();
                
                foreach (var neighbour in node.Neighbours)
                {
                    if (dict.ContainsKey(neighbour))
                        continue;

                    dict.Add(neighbour, node);
                    neighboursToUpdate.Add(neighbour);
                }
                
                foreach (var neighbour in neighboursToUpdate)
                {
                    UpdateNeighbours(neighbour);
                }
            }
        }

        public static int DistanceTo(this IMapNode node, IMapNode other)
        {
            var pathsToOther = other.CalculateShortestPaths();

            var steps = 0;

            var current = node;

            while (current is not null && current != other)
            {
                current = pathsToOther[current];
                steps++;
            }

            return current is null
                ? -1
                : steps;
        }
        
        public static IEnumerable<IMapNode> ContestedNodes(this IMap map)
        {
            return map.Nodes.Where(node => node.IsContested());
        }
    }
}