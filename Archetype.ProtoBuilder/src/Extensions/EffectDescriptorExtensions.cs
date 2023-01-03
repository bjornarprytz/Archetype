using System.Text;
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
    
    public static string GetStaticRulesText(this IEffectDescriptor effectDescriptor)
    {
        var sb = new StringBuilder(effectDescriptor.RulesTemplate);

        foreach (var (operand, parameterIndex) in effectDescriptor.Operands.Select((o, i) => (o,i)))
        {
            sb.Replace($"{{{parameterIndex}}}", $"{{<{parameterIndex}>.{operand.Description}}}");
        }

        return sb.ToString();
    }
    
    public static string GetDynamicRulesText(this IEffectDescriptor effectDescriptor, IContext context)
    {
        var sb = new StringBuilder(effectDescriptor.RulesTemplate);

        foreach (var ( operand,  parameterIndex) in effectDescriptor.Operands.Select((o, i) => (o,i)))
        {
            sb.Replace($"{{{parameterIndex}}}", $"{{{operand.ComputeValue(context)}}}");
        }

        return sb.ToString();
    }
}