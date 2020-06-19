
using System.Collections.Generic;

namespace Archetype
{
    public class PreSelectionInfo : TargetInfo
    {
        public override bool IsAutomatic => true;

        public PreSelectionInfo(params ITarget[] targets)
        {
            _minTargets = _maxTargets = targets.Length;

            _options = new List<ITarget>(targets);

            foreach(var target in targets)
            {
                Add(target);
            }
        }
    }
}
