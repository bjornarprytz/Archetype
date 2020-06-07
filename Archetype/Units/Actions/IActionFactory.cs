using System;
using System.Collections.Generic;

namespace Archetype
{
    public interface IActionFactory
    {
        IEnumerable<ActionInfo> MakeAction(Unit source, TargetInfo targets, GameState gameState);
    }
}
