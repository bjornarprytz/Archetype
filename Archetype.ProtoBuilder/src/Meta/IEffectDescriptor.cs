
using Archetype.Core.Effects;

namespace Archetype.Components.Meta;

internal interface IEffectDescriptor
{
    ITargetDescriptor? MainTarget { get; } // This is only populated if the effect is applied to a target. TargetDescriptors may also be in the Operands
    string RulesTemplate { get; }
    IEnumerable<IEffectParameter> Operands { get; }
}