namespace Archetype.Play.Factory;

public interface IFactory<out T>
{
    T Create();
}

public class Factory<T> : IFactory<T>
{
    private readonly Func<T> _factoryFunc;

    public Factory(Func<T> factoryFunc)
    {
        _factoryFunc = factoryFunc;
    }
    
    public T Create()
    {
        return _factoryFunc();
    }
}