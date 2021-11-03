using System.Collections;
using System.Collections.Generic;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IHand  : IZone<ICard>
    {
        IEnumerable<ICard> Cards => Contents;
    }
}
