namespace Archetype.Builder.Builders.Base;

public interface IBuilder<out T> : IBuilder
{
    T Build();
}

public interface IBuilder{}