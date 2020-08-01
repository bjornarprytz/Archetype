using System;
using System.Collections.Generic;

namespace Archetype
{
    public interface IActionFactory
    {
        IEnumerable<ActionInfo> CreateAction(ISource source, ISelectionInfo<ITarget> targets, GameState gameState);
    }
}
