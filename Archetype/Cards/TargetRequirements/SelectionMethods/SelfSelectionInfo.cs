
using System.Collections.Generic;

namespace Archetype
{
    public class SelfSelectionInfo : TargetInfo
    {
        public override bool IsAutomatic => true;

        public SelfSelectionInfo(ITarget self)
        {
            _options.Add(self);
            _minTargets = _maxTargets = 1;
            Add(self);
        }
    }
}
