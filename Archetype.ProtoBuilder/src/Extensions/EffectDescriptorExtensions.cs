using Archetype.Components.Meta;
using Archetype.Core.Effects;

namespace Archetype.Components.Extensions;

internal static class EffectDescriptorExtensions
{
    public static IEnumerable<ITargetDescriptor> GetTargets(this IEffectDescriptor effectDescriptor)
    {
        if (effectDescriptor.MainTarget != null)
            yield return effectDescriptor.MainTarget;

        foreach (var targetInOperands in effectDescriptor.Operands.SelectMany(operand => operand.GetTargets()))
        {
            yield return targetInOperands;
        }
    }
}