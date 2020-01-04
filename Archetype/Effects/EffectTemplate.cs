using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public abstract class EffectTemplate
    {
        public delegate IEnumerable<Unit> TargetOptions(GameLoop gameLoop);

        protected TargetParams<Unit> TargetParams { get; private set; }


        protected EffectTemplate(TargetParams<Unit> requirements, TargetOptions targetPool)
        {
            TargetParams = requirements;
            TargetPool = targetPool;
        }

        protected TargetOptions TargetPool { get; private set; }

        internal EffectArgs Args(Unit owner, GameLoop gameLoop) => TargetParams.GetArgs(owner, TargetPool(gameLoop));
 

        public abstract Effect CreateEffect(EffectArgs args);
    }
}
