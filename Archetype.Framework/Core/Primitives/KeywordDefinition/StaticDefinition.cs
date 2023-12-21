namespace Archetype.Framework.Core.Primitives;

public abstract class StaticDefinition<T> : KeywordDefinition
{
    protected override OperandDeclaration<T> OperandDeclaration { get; } = new();
}