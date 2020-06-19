using System.Collections;
using System.Collections.Generic;

namespace Archetype
{
    public class AllSelectionInfo : TargetInfo
    {
        public override bool IsAutomatic => true;

        public AllSelectionInfo(IEnumerable<ITarget> options)
        {
            _options = new List<ITarget>(options);

            foreach (var target in _options)
            {
                Add(target);
            }
        }
    }
}
