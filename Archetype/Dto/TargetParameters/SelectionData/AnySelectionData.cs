using System.Collections.Generic;

namespace Archetype
{
    public class AnySelectionData : TargetSelectionData
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public override ISelectionInfo<ITarget> GetSelectionInfo(IEnumerable<ITarget> options)
        {
            return new AnySelectionInfo(Min, Max, options);
        }
    }
}
