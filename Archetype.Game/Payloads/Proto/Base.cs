using System;
using Archetype.View.Proto;

namespace Archetype.Game.Payloads.Proto
{

    public abstract class ProtoData : IProtoDataFront
    {
        public string Name { get; set; }
    }
}