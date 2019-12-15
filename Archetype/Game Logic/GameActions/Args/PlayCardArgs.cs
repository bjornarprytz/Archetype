using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public class PlayCardArgs : MultipleChoicesArgs<EffectArgs>
    {
        public List<EffectArgs> EffectArgs => SubChoices;
    }
}
