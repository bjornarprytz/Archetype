using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public interface ITargetRequirements
    {
        TargetInfo GetRequirements(GameState gameState);
    }
}
