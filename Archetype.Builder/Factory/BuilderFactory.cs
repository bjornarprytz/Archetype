using System;
using Archetype.Builder.Builders;
using Archetype.Builder.Builders.Base;

namespace Archetype.Builder.Factory
{
    public interface IBuilderFactory
    {
        T Create<T>() where T : class, IBuilder;
    }
    
    public class BuilderFactory : IBuilderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public BuilderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        
        public T Create<T>() where T : class, IBuilder
        {
            return _serviceProvider.GetService(typeof(T)) as T;
        }
    }
}
