using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class QualifiedSelector<T>
    {
        public SelectorPreference Preference { get; set; }
        public ValueProvider<T, int> ValueProvider { get; set; }

        public T SelectOption(IEnumerable<T> options)
        {
            return Preference.Select(options, ValueProvider);
        }
    }
}
