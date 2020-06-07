using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public abstract class ActionParameterData
    {
        public TargetRequirementData TargetRequirements { get; set; }

        internal IEnumerable<ActionInfo> GetArgs(Unit source, TargetInfo targets, GameState gameState) => targets.ChosenTargets.Select(target => GetActionInfo(source, target, gameState));

        protected abstract ActionInfo GetActionInfo(Unit source, ITarget target, GameState gameState);
    }
}
