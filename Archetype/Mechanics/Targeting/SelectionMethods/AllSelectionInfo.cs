using System.Collections;
using System.Collections.Generic;

namespace Archetype
{
    public class AllSelectionInfo<T> : SelectionInfo<T>
    {
        public override bool IsAutomatic => true;

        public AllSelectionInfo(IEnumerable<T> options) : base(options)
        {
            foreach (var option in _options)
            {
                Add(option);
            }
        }
    }
}
