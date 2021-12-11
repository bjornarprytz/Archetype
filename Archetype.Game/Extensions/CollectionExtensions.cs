using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Archetype.Game.Extensions
{
    public static class CollectionExtensions
    {
        private static readonly Random Random = new (); // TODO: Consider using a seed here
        
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> collection)
        {
            var array = collection.ToArray();
            
            var n = array.Length;
            for (var i = 0; i < (n - 1); i++)
            {
                var r = i + Random.Next(n - i);
                
                (array[r], array[i]) = (array[i], array[r]);
            }

            return array;
        }
        
        public static bool IsEmpty<T>(this IEnumerable<T> items)
        {
            return !items.Any();
        }
    }
}