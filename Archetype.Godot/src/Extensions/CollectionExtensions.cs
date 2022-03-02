using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Archetype.Godot.Extensions
{
    public static class CollectionExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> stuff)
        {
            return !stuff.Any();
        }
    }
}