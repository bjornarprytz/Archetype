
using Archetype.Core.Effects;

namespace Archetype.Components.Meta;

internal interface IEffectParameter
{
    IEnumerable<ITargetDescriptor> GetTargets(); // This is only populated if the effect parameter requires something from a target
    string Description { get; }
    string ComputeValue(IContext context);
}