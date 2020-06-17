using System.Collections.Generic;

namespace Archetype
{
    public class RandomSelectionData : TargetSelectionData
    {
        public int Max { get; set; }

        public override ISelectionInfo<ITarget> GetSelectionInfo(IEnumerable<ITarget> options)
        {
            return new RandomSelectionInfo(Max, options);
        }
    }
}
