using System.Collections.Generic;

namespace Archetype
{
    public class RandomSelectionInfo<T> : SelectionInfo<T>
    {
        public override bool IsAutomatic => true;

        public RandomSelectionInfo(int n, IEnumerable<T> options) : base(options)
        {
            foreach (var choice in _options.GrabRandom(n))
            {
                Add(choice);
            }
        }
    }
}
