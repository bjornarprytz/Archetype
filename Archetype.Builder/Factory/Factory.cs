using System;

namespace Archetype.Builder.Factory;

public interface IFactory<out T> { T Create(); }

internal class Factory<T> : IFactory<T>
{
    private readonly Func<T> _initFunc;

    public Factory(Func<T> initFunc)
    {
        _initFunc = initFunc;
    }

    public T Create()
    {
        return _initFunc();
    }
}