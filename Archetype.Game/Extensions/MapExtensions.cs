using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Aqua.EnumerableExtensions;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.MetaData;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Extensions
{
    public static class MapExtensions
    {
        public static IEnumerable<ICreature> EachEnemyCreature(this IMap map)
        {
            return map.Nodes.SelectMany(Enemy<ICreature>);
        }
        
        public static IEnumerable<ICreature> EachFriendlyCreature(this IMap map)
        {
            return map.Nodes.SelectMany(Friendly<ICreature>);
        }
        
        public static IEnumerable<IStructure> EachFriendlyStructure(this IMap map)
        {
            return map.Nodes.SelectMany(Friendly<IStructure>);
        }
        
        public static IEnumerable<IStructure> EachEnemyStructure(this IMap map)
        {
            return map.Nodes.SelectMany(Enemy<IStructure>);
        }

        public static IEnumerable<T> Friendly<T>(this IMapNode node)
            where T : IUnit
        {
            return node.Contents.OfType<T>()
                .Where(c => c.IsFriendly());
        }
        
        public static IEnumerable<T> Enemy<T>(this IMapNode node)
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
        
        public static bool IsContested(this IMapNode node)
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
    }
}