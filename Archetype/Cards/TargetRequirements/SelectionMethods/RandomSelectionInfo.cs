
using System.Collections;
using System.Collections.Generic;

namespace Archetype
{
    public class RandomSelectionInfo : TargetInfo
    {
        public override bool IsAutomatic => true;

        public RandomSelectionInfo(int n, IEnumerable<ITarget> options)
        {
            _options = new List<ITarget>(options);

            foreach (var target in _options.GrabRandom(n))
            {
                Add(target);
            }
        }
    }
}
