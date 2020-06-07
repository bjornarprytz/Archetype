using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class PlayCardArgs
    {
        public IList<TargetInfo> TargetInfos { get; set; }

        public bool Valid => TargetInfos.All(targetInfo => targetInfo.Valid);
    }
}
