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
            switch (Preference)
            {
                case SelectorPreference.Highest:
                    return options.OrderBy(option => ValueProvider.GetValue(option)).FirstOrDefault();
                case SelectorPreference.Lowest:
                    return options.OrderByDescending(option => ValueProvider.GetValue(option)).FirstOrDefault();
                case SelectorPreference.Random:
                    return options.GrabRandom(1).FirstOrDefault();
                default:
                    throw new Exception($"Unhandled SelectorPreference {Preference}");
            }
        }
    }
}
