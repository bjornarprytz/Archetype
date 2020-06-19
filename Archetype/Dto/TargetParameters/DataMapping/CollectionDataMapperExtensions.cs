using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public static class CollectionDataMapperExtensions
    {
        public static T Select<T, V>(this SelectorPreference preference, IEnumerable<T> options, ValueProvider<T, V> valueProvider)
            where V : IComparable
        {
            switch (preference)
            {
                case SelectorPreference.High:
                    return options.OrderByDescending(option => valueProvider.GetValue(option)).FirstOrDefault();
                case SelectorPreference.Low:
                    return options.OrderBy(option => valueProvider.GetValue(option)).FirstOrDefault();
                case SelectorPreference.Random:
                    return options.GrabRandom(1).FirstOrDefault();
                default:
                    throw new Exception($"Unhandled SelectorPreference {preference}");
            }
        }

        public static int Compute<T>(this AggregatorType aggregator, IEnumerable<T> set, ValueProvider<T, int> valueProvider)
        {
            switch (aggregator)
            {
                case AggregatorType.Average:
                    return (int)set.Average(o => valueProvider.GetValue(o));
                case AggregatorType.Count:
                    return set.Count();
                case AggregatorType.High:
                    return set.Select(o => valueProvider.GetValue(o))
                        .OrderByDescending(x=>x).FirstOrDefault();
                case AggregatorType.Low:
                    return set.Select(o => valueProvider.GetValue(o))
                        .OrderBy(x=>x).FirstOrDefault();
                case AggregatorType.Random:
                    return set.Select(o => valueProvider.GetValue(o))
                        .GrabRandom(1).FirstOrDefault();
                case AggregatorType.Sum:
                    return set.Sum(o => valueProvider.GetValue(o));
                default:
                    throw new Exception($"Unhandled AggregatorType {aggregator}");

            }
        }
    }
}
