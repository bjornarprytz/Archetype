using System.Collections.Generic;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Context.Card
{
    public interface ICardPlayArgs
    {
        IPlayer Player { get; }
        ICard Card { get; }
        IMapNode Whence { get; }
        IEnumerable<IGameAtom> Targets { get; }
    }
}