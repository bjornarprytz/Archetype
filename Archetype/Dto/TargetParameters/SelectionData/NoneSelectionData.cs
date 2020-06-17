using System.Collections.Generic;

namespace Archetype
{
    public class NoneSelectionData : TargetSelectionData
    {
        public override ISelectionInfo<ITarget> GetSelectionInfo(IEnumerable<ITarget> options)
        {
            return new NoneSelectionInfo();
        }
    }
}
