using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class PlayCardArgs
    {
        public IList<ISelectionInfo<ITarget>> TargetInfos { get; set; }

        public PlayCardArgs(IList<ISelectionInfo<ITarget>> targetInfos)
        {
            TargetInfos = targetInfos;
        }

        public bool Valid => TargetInfos.All(targetInfo => targetInfo.IsValid);
    }
}
