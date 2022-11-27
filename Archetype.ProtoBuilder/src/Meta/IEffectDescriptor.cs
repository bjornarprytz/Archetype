using Archetype.Core.Effects;

namespace Archetype.Components.Meta;

internal interface IEffectDescriptor
{
    ITargetDescriptor? MainTarget { get; }
    string Keyword { get; }
    IEnumerable<IEffectParameter> Operands { get; }
}