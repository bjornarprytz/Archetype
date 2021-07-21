using System;
using System.Collections.Generic;

namespace Archetype.Core
{
    public interface IHand
    {
        IEnumerable<ICard> Cards { get; }
    }
}
