using System;
using System.Collections.Generic;
using Archetype.Game.Payloads.Metadata;

namespace Archetype.Game.Payloads.Proto
{
    public interface ICardProtoData
    {
        Guid Id { get; }
        
        int Cost { get; }
        CardMetaData MetaData { get; }
        IEnumerable<ITarget> Targets { get; }
        IEnumerable<IEffect> Effects { get; }
    }
}