using System.Collections;
using System.Collections.Generic;

namespace Archetype
{
    public class ForcedSelectionInfo<T> : SelectionInfo<T>
    {
        public override bool IsAutomatic => Options.Count <= _maxChoices;

        public ForcedSelectionInfo(int x, IEnumerable<T> options) : base(options)
        {
            _maxChoices = x;

            if (IsAutomatic)
            {
                foreach(var choice in options)
                {
                    Add(choice);
                }
            }
        }
    }
}
