using System.Collections.Generic;
using Archetype.Game.Payloads.Context.Effect.Base;

namespace Archetype.Game.Payloads.Context;

public interface IEffectProvider
{
    IEnumerable<IEffect> Effects { get; }
}