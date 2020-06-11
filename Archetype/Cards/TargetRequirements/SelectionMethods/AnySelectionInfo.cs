
using System.Collections.Generic;

namespace Archetype
{
    public class AnySelectionInfo : TargetInfo
    {
        public override bool IsAutomatic => false;

        public AnySelectionInfo(int min, int max, IEnumerable<ITarget> options)
        {
            _options = new List<ITarget>(options);

            _minTargets = min;
            _maxTargets = max;
        }
    }
}
