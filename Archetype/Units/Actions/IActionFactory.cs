using System;
using System.Collections.Generic;

namespace Archetype
{
    public interface IActionFactory
    {
        IEnumerable<ActionInfo> CreateAction(Unit source, ITargetSelectInfo targets, GameState gameState);
    }
}
