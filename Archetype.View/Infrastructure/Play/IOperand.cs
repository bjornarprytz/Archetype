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