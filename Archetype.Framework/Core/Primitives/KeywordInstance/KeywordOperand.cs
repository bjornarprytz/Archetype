﻿namespace Archetype.Framework.Core.Primitives;

public interface IKeywordOperand
{
    Type Type { get; }
    Func<IResolutionContext?, object?> GetValue { get; }
}

public interface IKeywordOperand<out T> : IKeywordOperand
{
    new Func<IResolutionContext?, T> GetTypedValue { get; }
}

public record KeywordOperand(Type Type, Func<IResolutionContext?, object?> GetValue) : IKeywordOperand
{
    public KeywordOperand(Type type, object? value) : this(type, (_ => value))
    { }
}

public record KeywordOperand<T> : KeywordOperand, IKeywordOperand<T>
{
    public KeywordOperand(Func<IResolutionContext?, T> getTypedValue) : base(typeof(T),
        ctx => getTypedValue(ctx))
    {
        GetTypedValue = getTypedValue;
    }
    
    public KeywordOperand(T value) : this(_ => value)
    { }
    public Func<IResolutionContext?, T> GetTypedValue { get; }
}