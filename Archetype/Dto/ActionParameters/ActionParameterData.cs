using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public abstract class ActionParameterData : IActionFactory
    {
        public TargetRequirementData TargetRequirements { get; set; }

        public IEnumerable<ActionInfo> MakeAction(Unit source, TargetInfo targets, GameState gameState) => targets.ChosenTargets.Select(target => GetActionInfo(source, target, gameState));

        protected abstract ActionInfo GetActionInfo(Unit source, ITarget target, GameState gameState);
    }
}
