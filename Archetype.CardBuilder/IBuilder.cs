namespace Archetype.CardBuilder
{
    public interface IBuilder { }

    public interface IBuilder<out T> : IBuilder
        where T : new()
    {
        T Build();
    }

    public abstract class BaseBuilder<T> : IBuilder<T>
        where T : new()
    {
        protected BaseBuilder()
        {
            Construction = new T();
        }
        internal T Construction { get; set; }
        protected abstract void PreBuild();
        public T Build() {
            PreBuild();
            return Construction;
        }        
    }
}
