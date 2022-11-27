using Archetype.Core.Atoms;
using Archetype.Core.Effects;

namespace Archetype.Components.Meta;

internal interface IEffectParameter<in TContext> : IEffectParameter
    where TContext : IContext
{
    string ComputeValue(TContext context);
}

internal interface IEffectParameter
{
    IEnumerable<ITargetDescriptor> GetTargets();
    string Description { get; }
    string ComputeValue(IContext context);
}