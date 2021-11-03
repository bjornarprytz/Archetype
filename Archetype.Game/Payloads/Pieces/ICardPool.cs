using System;
using System.Collections.Generic;
using Archetype.Core;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Pieces
{
    public interface ICardPool
    {
        ICardProtoData this[Guid guid] { get; }
        IEnumerable<ICardProtoData> Cards { get; }
    }
}