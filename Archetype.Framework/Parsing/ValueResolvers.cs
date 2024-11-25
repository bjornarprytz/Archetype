using Archetype.Framework.Core;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework.Parsing;

internal record AtomPredicate : IAtomPredicate
{
    public AtomPredicate(IAtomValue atomValue, ComparisonOperator @operator, IContextValue compareValue)
    {
        AtomValue = atomValue;
        Operator = @operator;
        CompareValue = compareValue;
        
        if (atomValue.ValueType != compareValue.ValueType)
            throw new InvalidOperationException($"Mismatched value types: {atomValue.ValueType} != {compareValue.ValueType}");

        if (atomValue.ValueType == typeof(string) && @operator is not (ComparisonOperator.Equal or ComparisonOperator.NotEqual))
            throw new InvalidOperationException($"Invalid comparison operator for string: {@operator}");

        if (atomValue.ValueType == typeof(int) &&
            @operator is ComparisonOperator.Contains or ComparisonOperator.NotContains)
        {
            throw new InvalidOperationException($"Invalid comparison operator for int: {@operator}");
        }
        
        if (atomValue.ValueType == typeof(IEnumerable<IAtom>) &&
            @operator is not (ComparisonOperator.Contains or ComparisonOperator.NotContains))
        {
            throw new InvalidOperationException($"Invalid comparison operator for IEnumerable<IAtom>: {@operator}");
        }
    }

    public IAtomValue AtomValue { get; init; }
    public ComparisonOperator Operator { get; init; }
    public IContextValue CompareValue { get; init; }
}

internal record ReferenceNumber : ContextValue<int?>, INumber
{
    public ReferenceNumber(IEnumerable<string> path) : base(path) { }
    int? IValue<IValueWhence, int?>.GetValue(IValueWhence context) => WrapAccessor(context);
}

internal record ReferenceWord : ContextValue<string?>, IWord
{
    public ReferenceWord(IEnumerable<string> path) : base(path){}

    string? IValue<IValueWhence, string?>.GetValue(IValueWhence context) => WrapAccessor(context);
}

internal record ReferenceGroup : ContextValue<IEnumerable<IAtom>>, IGroup
{
    public ReferenceGroup(IEnumerable<string> path) : base(path){}
    
    IEnumerable<IAtom>? IValue<IValueWhence, IEnumerable<IAtom>?>.GetValue(IValueWhence context) => WrapAccessor(context); 
}

internal record ReferenceValue : ContextValue<object?>, IContextValue, IValue
{
    public ReferenceValue(IEnumerable<string> path) : base(path) { }
    object? IValue<IValueWhence, object?>.GetValue(IValueWhence context) => WrapAccessor(context);
}



internal record ImmediateWord : ImmediateValue<string>, IWord
{
    public ImmediateWord(string value) : base(value) { }
}

internal record ImmediateNumber : ImmediateValue<int?>, INumber
{
    public ImmediateNumber(int value) : base(value) { }
}


internal record AtomGroup : AtomValue<IEnumerable<IAtom>>, IGroup
{
    public AtomGroup(IEnumerable<string> path) : base(path){}

    IEnumerable<IAtom>? IValue<IValueWhence, IEnumerable<IAtom>>.GetValue(IValueWhence context) => WrapAccessor(context);
}

internal record AtomNumber : AtomValue<int?>, INumber
{
    public AtomNumber(IEnumerable<string> path) : base(path){}

    int? IValue<IValueWhence, int?>.GetValue(IValueWhence context) => WrapAccessor(context);
}

internal record AtomWord : AtomValue<string?>, IWord
{
    public AtomWord(IEnumerable<string> path) : base(path){}

    string? IValue<IValueWhence, string?>.GetValue(IValueWhence context) => WrapAccessor(context);
}

internal record AtomValue : AtomValue<object?>, IAtomValue, IValue{
    public AtomValue(IEnumerable<string> path) : base(path){ }
    object? IValue<IValueWhence, object?>.GetValue(IValueWhence context) => WrapAccessor(context);
}


internal abstract record ContextValue<TValue> : IContextValue<TValue>
{
    private readonly Func<IResolutionContext, TValue?> _valueAccessor;
    protected ContextValue(IEnumerable<string> path)
    {
        Path = path.ToArray();
        ValueType = Path.GetValueType<IResolutionContext>();
        
        if (!typeof(TValue).IsAssignableFrom(ValueType))
            throw new InvalidOperationException($"Invalid value type for group: {ValueType}");
        
        _valueAccessor = Path.CreateAccessor<IResolutionContext, TValue>();
    }
    
    public TValue? Immediate => default;
    public Type ValueType { get; }
    public TValue? GetValue(IResolutionContext context) => _valueAccessor(context);
    

    public Whence Whence => Whence.Context;
    public string[] Path { get; }
    
    protected TValue? WrapAccessor(IValueWhence whence)
    {
        return whence switch
        {
            IResolutionContext typedWhence => GetValue(typedWhence),
            _ => throw new InvalidOperationException(
                $"Invalid context type: {whence.GetType()}. Expected: {typeof(IResolutionContext)}")
        };
    }
}

internal abstract record ImmediateValue<TValue> : IValue<IValueWhence, TValue>
{
    protected ImmediateValue(TValue value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));
        
        Immediate = value;
    }

    public Whence Whence => Whence.Immediate;
    public TValue? Immediate { get; }
    public Type ValueType => typeof(TValue);
    public TValue GetValue(IValueWhence context) => Immediate!;
    public string[]? Path => null;
}

internal abstract record AtomValue<TValue> : IAtomValue<TValue>
{
    private readonly Func<IAtom, TValue?> _valueAccessor;
    protected AtomValue(IEnumerable<string> path)
    {
        Path = path.ToArray();
        ValueType = Path.GetValueType<IAtom>();
        
        if (!typeof(TValue).IsAssignableFrom(ValueType))
            throw new InvalidOperationException($"Invalid value type for group: {ValueType}");
        
        _valueAccessor = Path.CreateAccessor<IAtom, TValue>();
    }
    
    public TValue? Immediate => default;
    public Type ValueType { get; }
    public TValue? GetValue(IAtom context) => _valueAccessor(context);
    

    public Whence Whence => Whence.Atom;
    public string[] Path { get; }
    
    protected TValue? WrapAccessor(IValueWhence whence)
    {
        return whence switch
        {
            IAtom typedWhence => GetValue(typedWhence),
            _ => throw new InvalidOperationException(
                $"Invalid context type: {whence.GetType()}. Expected: {typeof(IResolutionContext)}")
        };
    }
}