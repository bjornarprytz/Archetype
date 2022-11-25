using OneOf;

namespace Archetype.Components.Meta;

internal interface IEffectParameter
{
    OneOf<IImmediateValue, ITargetProperty, IContextProperty, IAggregateProperty> Value { get; }
}

internal interface IImmediateValue
{
    string? Value { get; }
}

internal interface ITargetProperty
{
    Type TargetType { get; }
    int TargetIndex { get; }
    string PropertyPath { get; }
}

internal interface IContextProperty
{
    string PropertyPath { get; }
}

internal interface IAggregateProperty
{
    string Description { get; }
    string PropertyPath { get; }
}