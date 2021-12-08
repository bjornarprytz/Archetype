using System.Collections;
using Aqua.EnumerableExtensions;

namespace Archetype.Builder.Extensions
{
    public static class LogicExtensions
    {
        public static bool IsEmpty(this IEnumerable items)
        {
            return !items.Any();
        }

        public static bool IsMissing(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
    }
}