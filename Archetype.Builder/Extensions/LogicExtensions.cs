using System.Collections;
using Aqua.EnumerableExtensions;

namespace Archetype.Builder.Factory
{
    public static class LogicExtensions
    {
        public static bool IsEmpty(this IEnumerable items)
        {
            return !items.Any();
        }
    }
}