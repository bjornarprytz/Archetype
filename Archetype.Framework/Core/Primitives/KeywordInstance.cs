using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public interface IKeywordInstance
{
    Guid Id { get; }
    string Keyword { get; }
    IReadOnlyList<KeywordOperand> Operands { get; }
}

public record KeywordInstance : IKeywordInstance
{
    public Guid Id { get; } = Guid.NewGuid();
    public required string Keyword { get; init; } = null!;
    public IReadOnlyList<KeywordOperand> Operands { get; init; } = new List<KeywordOperand>();
}

public record KeywordTarget(
    Func<IResolutionContext, IAtom> GetTarget
);

public record KeywordOperand(Type Type, Func<IResolutionContext?, object?> GetValue);

public record KeywordOperand<T>(Func<IResolutionContext?, T> GetTypedValue) : KeywordOperand(typeof(T),
    ctx => GetTypedValue(ctx))
{
    public Func<IResolutionContext, T> GetTypedValue { get; init; } = GetTypedValue;
}

public record ImmediateKeywordOperand<T>(T Value) : KeywordOperand<T>(_ => Value)
{
    public T Value { get; init; } = Value;
}



public record ComputedPropertyKeywordOperand(int ComputedValueIndex) : KeywordOperand<int>(
    ctx => ctx?.ComputedValues[ComputedValueIndex] ?? throw new InvalidOperationException($"Cannot access computed property with index ({ComputedValueIndex}): Provided context is null.")
)
{
    public int ComputedValueIndex { get; init; } = ComputedValueIndex;
}
