using System.Collections.Generic;

namespace Archetype.Core
{
    public interface IDiscardPile
    {
        IEnumerable<ICard> Cards { get; set; }
    }
}
