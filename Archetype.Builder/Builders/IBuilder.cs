namespace Archetype.Builder.Builders
{
    public interface IBuilder<out T>
    {
        T Build();
    }

}
