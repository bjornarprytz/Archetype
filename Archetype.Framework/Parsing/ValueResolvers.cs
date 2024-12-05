using Archetype.Framework.Core;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework.Parsing;

internal record AtomPredicate<T> : IAtomPredicate<T>
{
    private readonly IAtomValue<T> _atomValue;
    private readonly IValue<IValueWhence, T> _compareValue;
    
    public AtomPredicate(IAtomValue<T> atomValue, string compareExpression, IValue<IValueWhence, T> compareValue)
    {
        // TODO: There's still a little messy when comparing groups. That case might need its own record because atomValue and compareValue are different types. (IEnumerable<IAtom> and IAtom)
        
        Operator =  compareExpression.ParseComparisonOperator();
        Operator.ValidateOrThrow<T>();
        
        _atomValue = atomValue;
        _compareValue = compareValue;
    }

    public IAtomValue AtomValue { get; init; }

    IAtomValue<T> IAtomPredicate<T>.AtomValue => _atomValue;

    public ComparisonOperator Operator { get; init; }

    IValue<IValueWhence, T> IAtomPredicate<T>.CompareValue => _compareValue;

    public bool Evaluate(IResolutionContext context, IAtom atom)
    {
        var atomValue = _atomValue.GetValue(atom);
        var compareValue = _compareValue.GetValue(context);

        return Operator.Compare(atomValue, compareValue);
    }

    public IValue CompareValue { get; init; }
    
    
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