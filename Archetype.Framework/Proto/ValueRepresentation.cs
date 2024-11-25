using Archetype.Framework.Parsing;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework.Core;

public interface IValueWhence
{
    /* Marker interface for types that can be the source of a value accessor */
}

public interface INumber : IValue<IValueWhence, int?>;
public interface IWord : IValue<IValueWhence, string?>;
public interface IGroup : IValue<IValueWhence, IEnumerable<IAtom>?>;

public interface IValue : IValue<IValueWhence, object?>;


public interface IContextValue<out TValue> : IValue<IResolutionContext, TValue> { }
public interface IContextValue : IContextValue<object?> { }


public interface IAtomValue<out TValue> : IValue<IAtom, TValue> { }
public interface IAtomValue : IAtomValue<object?> { }

public interface IValue<in TWhence, out TValue> where TWhence : IValueWhence
{
    Whence Whence { get; }
    TValue? Immediate { get; }
    string[]? Path { get; }
    Type ValueType { get; }
    TValue? GetValue(TWhence context);
}