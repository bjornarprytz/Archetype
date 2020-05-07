using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public class PlayCardArgs : MultipleChoicesArgs<EffectArgs>
    {
        public List<EffectArgs> EffectArgs => SubChoices;
        
        public IEnumerable<Effect> CreateEffects()
        {
            if (!Valid) throw new Exception("Trying to create effects with invalid parameters");


            return EffectArgs.Select(e => e.Effect.CreateEffect(e));
        }
    }
}
