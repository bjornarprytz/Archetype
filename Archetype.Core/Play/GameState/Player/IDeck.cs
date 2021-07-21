using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype.Core
{
    public interface IDeck
    {
        Stack<ICard> Cards { get; }

        void Shuffle();
    }
}
