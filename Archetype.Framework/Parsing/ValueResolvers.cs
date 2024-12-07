using Archetype.Framework.Core;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework.Parsing;

internal record AtomPredicate<T> : IAtomPredicate<T>
{
    public AtomPredicate(IValue<IAtom, T> atomValue, string compareExpression, IValue<IValueWhence, T> compareValue)
    {
        Operator =  compareExpression.ParseComparisonOperator();
        Operator.ValidateOrThrow(atomValue.ValueType, compareValue.ValueType);
        
        AtomValue = atomValue;
        CompareValue = compareValue;
        
        LeftType = atomValue.ValueType;
        RightType = compareValue.ValueType;
    }

    public IValue<IAtom, T> AtomValue { get; }

    public Type LeftType { get; }
    public ComparisonOperator Operator { get; init; }
    public Type RightType { get; }

    public IValue<IValueWhence, T> CompareValue { get; }

    public bool Evaluate(IResolutionContext context, IAtom atom)
    {
        var atomValue = AtomValue.GetValue(atom);
        var compareValue = CompareValue.GetValue(context);

        return Operator.Compare(atomValue, compareValue);
    }
}

internal record AtomGroupPredicate<T> : IAtomGroupPredicate<T>
{
    public AtomGroupPredicate(IAtomValue<IEnumerable<T>> atomValue, string compareExpression, IValue<IValueWhence, T> compareValue)
    {
        Operator =  compareExpression.ParseComparisonOperator();
        Operator.ValidateOrThrow(atomValue.ValueType, compareValue.ValueType);
        
        AtomValue = atomValue;
        CompareValue = compareValue;
        
        LeftType = atomValue.ValueType;
        RightType = compareValue.ValueType;
    }


    public Type LeftType { get; }
    public ComparisonOperator Operator { get; init; }
    public Type RightType { get; }

    public IAtomValue<IEnumerable<T>> AtomValue { get; }
    public IValue<IValueWhence, T> CompareValue { get; }

    public bool Evaluate(IResolutionContext context, IAtom atom)
    {
        var atomValue = AtomValue.GetValue(atom);
        var compareValue = CompareValue.GetValue(context);

        return Operator.Compare(atomValue, compareValue);
    }
}

internal record Value<TWhence, TValue> : IValue
    where TWhence : IValueWhence
{
    private readonly Func<TWhence, TValue?> _valueAccessor;
    public Value(string[] path)
    {
        Path = path;

        if (!ValueType.Implements(path.GetValueType<TWhence>()))
        {
            throw new InvalidOperationException($"Invalid value type: {path.GetValueType<TWhence>()}. Expected: {typeof(TValue)}");
        }
        
        _valueAccessor = path.CreateAccessor<TWhence, TValue>();
        
        if (typeof(TWhence) == typeof(IResolutionContext))
            Whence = Whence.Context;
        else if (typeof(TWhence) == typeof(IAtom))
            Whence = Whence.Atom;
        else
            throw new InvalidOperationException($"Invalid whence type: {typeof(TWhence)}");
    }

    public Whence Whence { get; }
    object? IValue<IValueWhence, object?>.Immediate => Immediate;

    public TValue Immediate => throw new InvalidOperationException("Immediate value not available for context values");
    public string[] Path { get; }
    public Type ValueType => typeof(TValue);
    public object? GetValue(IValueWhence context) => WrapAccessor(context);

    public TValue? GetValue(TWhence context) => _valueAccessor(context);
    
    private TValue? WrapAccessor(IValueWhence whence)
    {
        return whence switch
        {
            TWhence typedWhence => GetValue(typedWhence),
            _ => throw new InvalidOperationException(
                $"Invalid context type: {whence.GetType()}. Expected: {typeof(TWhence)}")
        };
    }
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

internal record ReferenceAtom : ContextValue<IAtom?>, IValue<IValueWhence, IAtom>
{
    public ReferenceAtom(IEnumerable<string> path) : base(path){}

    IAtom? IValue<IValueWhence, IAtom>.GetValue(IValueWhence context) => WrapAccessor(context);
}

internal record ReferenceGroup<T> : ContextValue<IEnumerable<T>>, IGroup<IValueWhence, T>
{
    public ReferenceGroup(IEnumerable<string> path) : base(path){}
    
    IEnumerable<T>? IValue<IValueWhence, IEnumerable<T>?>.GetValue(IValueWhence context) => WrapAccessor(context); 
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


internal record AtomGroup<T> : AtomValue<IEnumerable<T>>, IGroup<IAtom, T>
{
    public AtomGroup(IEnumerable<string> path) : base(path){}

    IEnumerable<T>? IValue<IAtom, IEnumerable<T>?>.GetValue(IAtom context) => WrapAccessor(context);
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


internal record ContextValue<TValue> : IContextValue<TValue>
{
    private readonly Func<IResolutionContext, TValue?> _valueAccessor;
    protected ContextValue(IEnumerable<string> path)
    {
        Path = path.ToArray();
        ValueType = Path.GetValueType<IResolutionContext>();
        
        if (!ValueType.Implements(typeof(TValue)))
            throw new InvalidOperationException($"Invalid value type: {ValueType}. Expected: {typeof(TValue)}");
        
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

internal abstract record ImmediateValue<TValue> : IValue
{
    protected ImmediateValue(TValue value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));
        
        Immediate = value;
    }

    public Whence Whence => Whence.Immediate;
    object? IValue<IValueWhence, object?>.Immediate => Immediate;

    public TValue? Immediate { get; }
    public Type ValueType => typeof(TValue);
    object? IValue<IValueWhence, object?>.GetValue(IValueWhence context)
    {
        return GetValue(context);
    }

    public TValue GetValue(IValueWhence context) => Immediate!;
    public string[]? Path => null;
}

internal record AtomValue<TValue> : IAtomValue<TValue>
{
    private readonly Func<IAtom, TValue?> _valueAccessor;
    public AtomValue(IEnumerable<string> path)
    {
        Path = path.ToArray();
        ValueType = Path.GetValueType<IAtom>();
        
        if (!ValueType.Implements(typeof(TValue)))
            throw new InvalidOperationException($"Invalid value type: {ValueType}. Expected: {typeof(TValue)}");
        
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