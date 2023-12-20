namespace Archetype.Framework.Core.Primitives;

public record ComputeRef(int ComputedValueIndex) : KeywordOperand<int>(ctx => ctx?.ComputedValues[ComputedValueIndex] ?? throw new InvalidOperationException($"Cannot access computed property with index ({ComputedValueIndex}): Provided context is null."));