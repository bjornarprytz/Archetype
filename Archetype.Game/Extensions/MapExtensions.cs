using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.Game.Extensions
{
    public static class MapExtensions
    {
        public static IEnumerable<ICreature> EachEnemyCreature(this IMap map)
        {
            return map.Nodes.SelectMany(n => n.Contents)
                .OfType<ICreature>()
                .Where(c => c.IsEnemy());
        }
        
        public static IEnumerable<IStructure> EachFriendlyStructure(this IMap map)
        {
            return map.Nodes.SelectMany(n => n.Contents)
                .OfType<IStructure>()
                .Where(c => c.IsFriendly());
        }
        
        public static IEnumerable<IStructure> EachEnemyStructure(this IMap map)
        {
            return map.Nodes.SelectMany(n => n.Contents)
                .OfType<IStructure>()
                .Where(c => c.IsEnemy());
        } 
    }
}