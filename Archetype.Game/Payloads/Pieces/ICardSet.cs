using System.Collections.Generic;

namespace Archetype.Game.Payloads.Pieces
{
    public interface ICardSet
    {
        string Name { get; }
        IEnumerable<ICard> Cards { get; }
    }
}