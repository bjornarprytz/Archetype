
using System.Collections.Generic;

namespace Archetype
{
    public class PreSelectionInfo<T> : SelectionInfo<T>
    {
        public override bool IsAutomatic => true;

        public PreSelectionInfo(params T[] choices) : base(choices)
        {
            _minChoices = _maxChoices = choices.Length;

            foreach(var choice in choices)
            {
                Add(choice);
            }
        }
    }
}
