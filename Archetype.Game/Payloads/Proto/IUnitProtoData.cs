using System;
using System.Collections.Generic;
using Archetype.Game.Payloads.Metadata;

namespace Archetype.Game.Payloads.Proto
{
    public interface IUnitProtoData
    {
        Guid Id { get; }
        
        int Health { get; }
        UnitMetaData MetaData { get; }
        IEnumerable<ICardProtoData> Cards { get; }
    }
}