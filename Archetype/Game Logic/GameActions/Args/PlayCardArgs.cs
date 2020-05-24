using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public class PlayCardArgs
    {
        public TargetInfo Targets { get; set; }

        public bool Valid => Targets.Valid;
    }
}
