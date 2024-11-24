using Archetype.Framework.State;

namespace Archetype.Framework.Core;

public interface INumber : IValue
{
    int? ImmediateValue { get; }
    
    new object? Immediate  => ImmediateValue;
    new Type ValueType => typeof(int);
}

public interface IWord : IValue
{
    string? ImmediateValue { get; }
    
    new object? Immediate  => ImmediateValue;
    new Type ValueType => typeof(string);
}

public interface IAtomGroup : IValue
{
    new object? Immediate => null;
    new Type ValueType => typeof(IEnumerable<IAtom>);
}

public interface IValue
{
    Whence Whence { get; }
    object? Immediate { get; }
    string[]? Path { get; }
    Type ValueType { get; }
}