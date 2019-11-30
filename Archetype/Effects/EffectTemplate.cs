using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public abstract class EffectTemplate
    {
        public TargetParams<Unit> TargetParams { get; private set; }

        protected EffectTemplate(TargetParams<Unit> requirements)
        {
            TargetParams = requirements;
        }

        public abstract Effect CreateEffect(Unit source, List<Unit> targets);
    }
}
