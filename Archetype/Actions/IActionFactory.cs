using System;
using System.Collections.Generic;

namespace Archetype
{
    public interface IActionFactory
    {
        IEnumerable<ActionInfo> CreateAction(Unit source, ISelectionInfo<ITarget> targets, GameState gameState);
    }
}
