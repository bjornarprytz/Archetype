using System;

namespace Archetype.Game.Payloads.Proto
{
    public interface IProtoData
    {
        Guid Guid { get; }
    }

    public abstract class ProtoData : IProtoData
    {
        protected ProtoData()
        {
            Guid = Guid.NewGuid();
        }

        public Guid Guid { get; }
    }
}