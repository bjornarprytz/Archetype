using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public abstract class EffectTemplate
    {
        public TargetParams<Unit> TargetParams { get; private set; }

        public Choose<Unit> TargetPrompt(Unit source, GameState gameState) => TargetParams.GetPrompt(source, gameState.ActiveUnits);

        protected EffectTemplate(TargetParams<Unit> requirements)
        {
            TargetParams = requirements;
        }

        public abstract Effect CreateEffect(Unit source, List<Unit> targets);
    }
}
