using Archetype.Components.Meta;
using Archetype.Core.Effects;

namespace Archetype.Components.Extensions;

internal static class TriggerDescriptorExtensions
{
    public static string GetStaticRulesText(this ITriggerDescriptor triggerDescriptor)
    {
        return
            $"""
            When {triggerDescriptor.ConditionDescriptor.Keyword}, 
            if {triggerDescriptor.ConditionDescriptor.Predicate}, 
            then {triggerDescriptor.TriggerEffectDescriptor.GetStaticRulesText()}.
            """;
    }
    
    public static string GetDynamicRulesText(this ITriggerDescriptor triggerDescriptor, IContext context)
    {
        return
            $"""
            When {triggerDescriptor.ConditionDescriptor.Keyword}, 
            if {triggerDescriptor.ConditionDescriptor.Predicate}, 
            then {triggerDescriptor.TriggerEffectDescriptor.GetDynamicRulesText(context)}.
            """;
    }
}