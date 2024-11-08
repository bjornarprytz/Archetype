using Archetype.Framework.Effects;
using Archetype.Framework.Resolution;

namespace Archetype.Framework.Core;

public record CardProto
{
    public string Name { get; init; }
}

public record EffectResolver
{
    // TODO: Should this function do both the binding and the resolution? 
    public Func<IResolutionContext, IEffectResult> ResolveEffect { get; init; }
}