using Archetype.Components.Meta;

namespace Archetype.Components.Extensions;

internal static class EffectDescriptorExtensions
{
    public static IEnumerable<ITargetProperty> GetTargets(this IEffectDescriptor effectDescriptor)
    {
        if (effectDescriptor.Affected.Description.Value is ITargetProperty targetInAffected)
        {
            yield return targetInAffected;
        }

        foreach (var targetInOperands in effectDescriptor.Operands.Select(operand => operand.Value.Value).OfType<ITargetProperty>())
        {
            yield return targetInOperands;
        }
    }
}