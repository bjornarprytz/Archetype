using System.Collections.Generic;

namespace Archetype
{
    public class AggregateSelector<T>
    {
        public AggregatorType Aggregation { get; set; }
        public ValueProvider<T, int> ValueProvider { get; set; }

        public int Compute(IEnumerable<T> options)
        {
            return Aggregation.Compute(options, ValueProvider);
        }
    }
}
