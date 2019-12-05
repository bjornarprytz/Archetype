using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public class CardArgs : ChoiceArgs
    {
        internal Card Card { get; private set; }
        public List<EffectArgs> EffectArgs { get; set; }

        public override bool Valid => EffectArgs.All(arg => arg.Valid);
    }
}
