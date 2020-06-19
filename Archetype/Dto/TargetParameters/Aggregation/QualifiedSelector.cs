using System;
using System.Collections.Generic;

namespace Archetype
{
    public class QualifiedSelector<T, V>
        where V : IComparable
    {
        public SelectorPreference Preference { get; set; }
        public ValueProvider<T, V> ValueProvider { get; set; }

        public T SelectOption(IEnumerable<T> options)
        {
            return Preference.Select(options, ValueProvider);
        }
    }
}
