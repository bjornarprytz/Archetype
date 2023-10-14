using Archetype.Framework.Definitions;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Proto;

public interface IKeywordInstance
{
    Guid Id { get; }
    string Keyword { get; }
    IReadOnlyList<KeywordOperand> Operands { get; }
    IReadOnlyList<KeywordTarget> Targets { get; }
}

public record KeywordInstance : IKeywordInstance
{
    private readonly string _keyword = "_UNINITIALIZED_";
    
    public string Keyword
    {
        get => _keyword;
        init => _keyword = value.ToUpper();
    }

    public Guid Id { get; } = Guid.NewGuid();
    public IReadOnlyList<KeywordOperand> Operands { get; init; } = new List<KeywordOperand>();
    
    public IReadOnlyList<KeywordTarget> Targets { get; init; } = new List<KeywordTarget>();
}

public record KeywordTarget(
    Func<IResolutionContext, IAtom> GetTarget
);

public record KeywordOperand(Type Type, Func<IResolutionContext, object?> GetValue);

public record KeywordOperand<T>(Func<IResolutionContext, T> GetTypedValue) : KeywordOperand(typeof(T),
    ctx => GetTypedValue(ctx))
{
    public Func<IResolutionContext, T> GetTypedValue { get; init; } = GetTypedValue;
}

public record ImmediateKeywordOperand<T>(T Value) : KeywordOperand<T>(_ => Value);
public record FilterKeywordOperand(Filter Filter) : KeywordOperand<IEnumerable<IAtom>>(Filter.ProvideAtoms);
public record ComputedPropertyKeywordOperand(int ComputedValueIndex) : KeywordOperand<int>(
    ctx => ctx.ComputedValues[ComputedValueIndex]
);
