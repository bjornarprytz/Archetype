using System.Globalization;
using Archetype.View.Extensions;
using OneOf;

namespace Archetype.View.Infrastructure;

public interface IOperand
{
    OneOf<IImmediateValue, ITargetProperty, IContextProperty, IAggregateProperty> Value { get; }
}

public interface IImmediateValue
{
    string Value { get; }
}

public interface ITargetProperty
{
    Type TargetType { get; }
    string TypeId { get; }
    int TargetIndex { get; }
    string PropertyPath { get; }
}

public interface IContextProperty
{
    string PropertyPath { get; }
}

public interface IAggregateProperty
{
    string Description { get; }
    string PropertyPath { get; }
}


public record Operand : IOperand
{
    public Operand(IOneOf oneOf)
    {
        Value = oneOf.Value switch
        {
            IImmediateValue iv => OneOf<IImmediateValue, ITargetProperty, IContextProperty, IAggregateProperty>
                .FromT0(iv),
            ITargetProperty tp => OneOf<IImmediateValue, ITargetProperty, IContextProperty, IAggregateProperty>
                .FromT1(tp),
            IContextProperty cp => OneOf<IImmediateValue, ITargetProperty, IContextProperty, IAggregateProperty>
                .FromT2(cp),
            IAggregateProperty ap => OneOf<IImmediateValue, ITargetProperty, IContextProperty, IAggregateProperty>
                .FromT3(ap),
            _ => throw new ArgumentException($"Cannot parse operand of value type {oneOf.Value} "),
        };
    }
    
    public Operand(IImmediateValue value)
    {
        Value = OneOf<IImmediateValue, ITargetProperty, IContextProperty, IAggregateProperty>.FromT0(value);
    }
    
    public Operand(ITargetProperty value)
    {
        Value = OneOf<IImmediateValue, ITargetProperty, IContextProperty, IAggregateProperty>.FromT1(value);
    }

    public Operand(IContextProperty value)
    {
        Value = OneOf<IImmediateValue, ITargetProperty, IContextProperty, IAggregateProperty>.FromT2(value);
    }

    public Operand(IAggregateProperty value)
    {
        Value = OneOf<IImmediateValue, ITargetProperty, IContextProperty, IAggregateProperty>.FromT3(value);
    }
    
    public OneOf<IImmediateValue, ITargetProperty, IContextProperty, IAggregateProperty> Value { get; }
}

public record ImmediateValue(string Value) : IImmediateValue;

public record TargetProperty(Type TargetType, int TargetIndex, string PropertyPath) : ITargetProperty
{
    public string TypeId => TargetType.ReadableFullName();
}

public record ContextProperty(string PropertyPath) : IContextProperty;

public record AggregateProperty(string Description, string PropertyPath) : IAggregateProperty;