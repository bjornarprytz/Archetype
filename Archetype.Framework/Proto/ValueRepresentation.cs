using Archetype.Framework.Parsing;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework.Core;

public interface IValueWhence
{
    /* Marker interface for types that can be the source of a value accessor */
}

public interface IGroup<in TWhence, out T>: IValue<TWhence, IEnumerable<T>?>
    where TWhence : IValueWhence;
public interface IValue<in TWhence, out TValue> : IValue<TValue>
    where TWhence : IValueWhence
{
    TValue? GetValue(TWhence context);
}

public interface IValue<out TValue> : IValue
{
    new TValue? GetValue(IValueWhence context);
    new TValue? Immediate { get; }
}

public interface IValue
{
    Whence Whence { get; }
    object? Immediate { get; }
    string[]? Path { get; }
    Type ValueType { get; }
    object? GetValue(IValueWhence context);
}
