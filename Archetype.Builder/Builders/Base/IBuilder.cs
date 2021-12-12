namespace Archetype.Builder.Builders
{
    public interface IBuilder<out T> : IBuilder
    {
        T Build();
    }

    public interface IBuilder{}
}
