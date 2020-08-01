using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public abstract class ActionParameterData : IActionFactory
    {
        public TargetRequirementData TargetRequirements { get; set; }

        public IEnumerable<ActionInfo> CreateAction(ISource source, ISelectionInfo<ITarget> targets, GameState gameState)
        {
            return targets.ConfirmedSelection.Select(target => GetActionInfo(source, target, gameState));
        }

        protected abstract ActionInfo GetActionInfo(ISource source, ITarget target, GameState gameState);
    }
}
