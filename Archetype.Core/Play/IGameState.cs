using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archetype.Core
{
    public interface IGameState
    {
        IPlayer Player { get; }

        IBoard Map { get; }
    }
}
