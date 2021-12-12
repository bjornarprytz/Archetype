using System;

namespace Archetype.Game.Payloads.Proto
{
    public interface IProtoData
    {
        string Name { get; }
    }

    public abstract class ProtoData : IProtoData
    {
        public string Name { get; set; }
    }
}