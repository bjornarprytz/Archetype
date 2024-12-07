using System.Collections;
using Archetype.Framework.Core;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework.Parsing;

internal record AtomPredicate<TAtom, TValue> : IAtomPredicate<TAtom, TValue>
    where TAtom : IAtom
{
    public AtomPredicate(IValue<TAtom, TValue> atomValue, string compareExpression, IValue<TValue> compareValue)
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
    public IValue<TAtom, TValue> AtomValue { get; }
    public IValue<TValue> CompareValue { get; }


    public bool Evaluate(IResolutionContext context, IAtom atom) => WrapAccessor(context, atom);
    public bool EvaluateTyped(IResolutionContext context, TAtom atom)
    {
        var atomValue = AtomValue.GetValue(atom);
        var compareValue = CompareValue.GetValue(context);

        return Operator.Compare(atomValue, compareValue);
    }
    
    private bool WrapAccessor(IResolutionContext context, IAtom atom)
    {
        return atom switch
        {
            TAtom typedAtom => EvaluateTyped(context, typedAtom),
            _ => throw new InvalidOperationException($"Invalid atom type: {atom.GetType()}. Expected: {typeof(TAtom)}")
        };
    }

}

internal record AtomGroupPredicate<TAtom, TItem> : IAtomGroupPredicate<TAtom, TItem>
    where TAtom : IAtom
{
    public AtomGroupPredicate(IGroup<TAtom, TItem> atomValue, string compareExpression, IValue<TItem> compareValue)
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
    public bool Evaluate(IResolutionContext context, IAtom atom) => WrapAccessor(context, atom);

    public bool EvaluateTyped(IResolutionContext context, TAtom atom)
    {
        var atomValue = AtomValue.GetValue(atom);
        var compareValue = CompareValue.GetValue(context);

        return Operator.Compare(atomValue, compareValue);
    }

    public IGroup<TAtom, TItem> AtomValue { get; }
    public IValue<TItem> CompareValue { get; }
    
    private bool WrapAccessor(IResolutionContext context, IAtom atom)
    {
        return atom switch
        {
            TAtom typedAtom => EvaluateTyped(context, typedAtom),
            _ => throw new InvalidOperationException($"Invalid atom type: {atom.GetType()}. Expected: {typeof(TAtom)}")
        };
    }
}

internal record Value<TWhence, TValue> : IValue<TWhence, TValue>
    where TWhence : IValueWhence
{
    private readonly Func<TWhence, TValue?> _valueAccessor;
    public Value(IEnumerable<string> path)
    {
        Path = path.ToArray();

        if (!Path.GetValueType<TWhence>().Implements(ValueType))
        {
            throw new InvalidOperationException($"Invalid value type: {Path.GetValueType<TWhence>()}. Expected: {typeof(TValue)}");
        }
        
        _valueAccessor = Path.CreateAccessor<TWhence, TValue>();
        
        if (typeof(TWhence).Implements(typeof(IResolutionContext)))
            Whence = Whence.Context;
        else if (typeof(TWhence).Implements(typeof(IAtom)))
            Whence = Whence.Atom;
        else
            throw new InvalidOperationException($"Invalid whence type: {typeof(TWhence)}");
    }

    public Whence Whence { get; }

    object? IValue.Immediate => Immediate;

    public TValue Immediate => throw new InvalidOperationException("Immediate value not available for context values");
    public string[] Path { get; }
    public Type ValueType => typeof(TValue);
    TValue? IValue<TValue>.GetValue(IValueWhence context) => WrapAccessor(context);
    public object? GetValue(IValueWhence context) => WrapAccessor(context);
    public TValue? GetValue(TWhence context) => _valueAccessor(context);
    
    protected TValue? WrapAccessor(IValueWhence whence)
    {
        return whence switch
        {
            TWhence typedWhence => GetValue(typedWhence),
            _ => throw new InvalidOperationException(
                $"Invalid context type: {whence.GetType()}. Expected: {typeof(TWhence)}")
        };
    }
}

internal record Group<TWhence, TItem> : Value<TWhence, IEnumerable<TItem>>, IGroup<TWhence, TItem>
    where TWhence : IValueWhence
{
    public Group(IEnumerable<string> path) : base(path) { }
}

internal record ImmediateWord : ImmediateValue<string>
{
    public ImmediateWord(string value) : base(value) { }
}

internal record ImmediateNumber : ImmediateValue<int?>
{
    public ImmediateNumber(int value) : base(value) { }
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
    object? IValue.Immediate => Immediate;

    public TValue? Immediate { get; }
    public Type ValueType => typeof(TValue);
    object? IValue.GetValue(IValueWhence context)
    {
        return GetValue(context);
    }

    public TValue GetValue(IValueWhence context) => Immediate!;
    public string[]? Path => null;
}