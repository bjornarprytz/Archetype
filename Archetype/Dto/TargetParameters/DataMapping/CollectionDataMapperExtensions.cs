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
            return preference switch
            {
                SelectorPreference.High     => options.OrderByDescending(option => valueProvider.GetValue(option)).FirstOrDefault(),
                SelectorPreference.Low      => options.OrderBy(option => valueProvider.GetValue(option)).FirstOrDefault(),
                SelectorPreference.Random   => options.GrabRandom(1).FirstOrDefault(),
                _                           => throw new Exception($"Unhandled SelectorPreference {preference}"),
            };
        }

        public static int Compute<T>(this AggregatorType aggregator, IEnumerable<T> set, ValueProvider<T, int> valueProvider)
        {
            return aggregator switch
            {
                AggregatorType.Average  => (int)set.Average(o => valueProvider.GetValue(o)),
                AggregatorType.Count    => set.Count(),
                AggregatorType.High     => set.Select(o => valueProvider.GetValue(o)).OrderByDescending(x => x).FirstOrDefault(),
                AggregatorType.Low      => set.Select(o => valueProvider.GetValue(o)).OrderBy(x => x).FirstOrDefault(),
                AggregatorType.Random   => set.Select(o => valueProvider.GetValue(o)).GrabRandom(1).FirstOrDefault(),
                AggregatorType.Sum      => set.Sum(o => valueProvider.GetValue(o)),
                _                       => throw new Exception($"Unhandled AggregatorType {aggregator}"),
            };
        }
    }
}
