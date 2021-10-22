using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype.Core
{
    public interface IDeck : IZone
    {
        ICard Draw();
        void Shuffle();
    }
}
