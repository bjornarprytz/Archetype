using System;
using System.Text.Json.Serialization;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.View;
using Archetype.View.Infrastructure;

namespace Archetype.Game.Payloads.Context.Card
{
    public class Target<TTarget> : ITargetDescriptor
        where TTarget : IGameAtom
    {
        public Type TargetType => typeof(TTarget);
        public string TypeId => TargetType.ReadableFullName();
    }
}