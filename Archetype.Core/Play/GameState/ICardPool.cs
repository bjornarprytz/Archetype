using System.Collections;
using System.Collections.Generic;

namespace Archetype.Core
{
    public interface ICardPool
    {
        IEnumerable<SetData> CardSets { get; }
    }
}