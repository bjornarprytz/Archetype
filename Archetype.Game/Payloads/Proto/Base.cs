using System;

namespace Archetype.Game.Payloads.Proto
{
    internal interface IProtoData
    {
        string Name { get; }
    }

    internal abstract class ProtoData : IProtoData
    {
        public string Name { get; set; }
    }
}