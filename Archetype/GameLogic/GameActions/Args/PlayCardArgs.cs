using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class PlayCardArgs
    {
        public IList<ITargetSelectInfo> TargetInfos { get; set; }

        public PlayCardArgs(IList<ITargetSelectInfo> targetInfos)
        {
            TargetInfos = targetInfos;
        }

        public bool Valid => TargetInfos.All(targetInfo => targetInfo.IsValid);
    }
}
