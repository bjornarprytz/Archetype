using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public abstract class ActionParameterData : IActionFactory
    {
        public TargetRequirementData TargetRequirements { get; set; }

        public IEnumerable<ActionInfo> CreateAction(Unit source, ITargetSelectInfo targets, GameState gameState) => targets.ConfirmedSelection.Select(target => GetActionInfo(source, target, gameState));

        protected abstract ActionInfo GetActionInfo(Unit source, ITarget target, GameState gameState);
    }
}
