
using System.Collections.Generic;

namespace Archetype
{
    public abstract class TargetSelectionData
    {
        public abstract ISelectionInfo<ITarget> GetSelectionInfo(IEnumerable<ITarget> options);
    }
}
