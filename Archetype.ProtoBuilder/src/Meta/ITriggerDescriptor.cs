namespace Archetype.Components.Meta;

internal interface ITriggerDescriptor
{
    public IConditionDescriptor ConditionDescriptor { get; }
    public IEffectDescriptor TriggerEffectDescriptor { get; }
}

internal interface IConditionDescriptor
{
    string Keyword { get; } // e.g. Damage
    string Predicate { get; } // e.g. if Damage.Amount > 6
}