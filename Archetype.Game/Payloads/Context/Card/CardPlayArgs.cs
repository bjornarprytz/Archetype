using System.Collections.Generic;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Atoms.Base;

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