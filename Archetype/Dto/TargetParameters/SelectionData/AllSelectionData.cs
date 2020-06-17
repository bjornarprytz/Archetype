using System.Collections.Generic;

namespace Archetype
{
    public class AllSelectionData : TargetSelectionData
    {
        public override ISelectionInfo<ITarget> GetSelectionInfo(IEnumerable<ITarget> options)
        {
            return new AllSelectionInfo(options);
        }
    }
}
