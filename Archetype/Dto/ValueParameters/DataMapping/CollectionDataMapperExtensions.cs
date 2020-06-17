using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public static class CollectionDataMapperExtensions
    {
        public static T Select<T, V>(this SelectorPreference preference, IEnumerable<T> options, ValueProvider<T, V> valueProvider)
        {
            switch (preference)
            {
                case SelectorPreference.Highest:
                    return options.OrderBy(option => valueProvider.GetValue(option)).FirstOrDefault();
                case SelectorPreference.Lowest:
                    return options.OrderByDescending(option => valueProvider.GetValue(option)).FirstOrDefault();
                case SelectorPreference.Random:
                    return options.GrabRandom(1).FirstOrDefault();
                default:
                    throw new Exception($"Unhandled SelectorPreference {preference}");
            }
        }
    }
}
