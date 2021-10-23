using System;

namespace Archetype.CardBuilder
{
    public interface IBuilder { }

    public interface IBuilder<out T> : IBuilder
    {
        T Build();
    }

    public abstract class BaseBuilder<T> : IBuilder<T>
    {
        private readonly Func<T> _factory;
        protected BaseBuilder(Func<T> factory)
        {
            _factory = factory;
            
            Construction = factory();
        }
        internal T Construction { get; set; }
        protected abstract void PreBuild();
        public Action<TBuilder> ToProvider<TBuilder>()
            where TBuilder : BaseBuilder<T>
        {
            return provider => provider.Construction = Construction;
        }
        public T Build() {
            PreBuild();
            return Construction;
        }        
    }
}
