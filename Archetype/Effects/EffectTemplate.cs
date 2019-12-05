using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public abstract class EffectTemplate
    {
        public delegate IEnumerable<Unit> TargetOptions(GameState gameState);

        protected TargetParams<Unit> TargetParams { get; private set; }


        protected EffectTemplate(TargetParams<Unit> requirements, TargetOptions targetPool)
        {
            TargetParams = requirements;
            TargetPool = targetPool;
        }

        protected TargetOptions TargetPool { get; private set; }

        internal EffectArgs Args(Unit owner, GameState gameState) => TargetParams.GetArgs(owner, TargetPool(gameState));
 

        public abstract Effect CreateEffect(EffectArgs args);
    }
}
