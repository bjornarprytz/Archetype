
using System.Collections.Generic;

namespace Archetype
{
    public class AnySelectionInfo<T> : SelectionInfo<T>
    {
        public override bool IsAutomatic => false;

        public AnySelectionInfo(int min, int max, IEnumerable<T> options) : base(options)
        {
            _minChoices = min;
            _maxChoices = max;
        }
    }
}
