using System;
using System.Collections.Generic;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Atoms.Base;

namespace Archetype.Game.Payloads.Context.Card
{
    public interface ICardPlayArgs
    {
        Guid CardGuid { get; }
        Guid WhenceGuid { get; }
        IEnumerable<Guid> TargetGuids { get; }
    }
}