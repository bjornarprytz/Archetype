using System;
using Archetype.Builder.Builders;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.MetaData;
using Archetype.Game.Payloads.Pieces.Base;

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
        /*
         * 
        public static SetBuilder SetBuilder(string name)
        {
            return new SetBuilder(name);
        }
        public static CardBuilder CardBuilder(CardMetaData template=default)
        {
            return new CardBuilder(template);
        }
        
        public static CardEffectBuilder<TTarget> EffectBuilder<TTarget>()
            where TTarget : IGameAtom
        {
            return new CardEffectBuilder<TTarget>();
        }
        
        public static CardEffectBuilder EffectBuilder()
        {
            return new CardEffectBuilder();
        }
        
        public static NodeBuilder NodeBuilder(Func<IInstanceFactory> getInstanceFactory)
        {
            return new NodeBuilder(getInstanceFactory);
        }

        public static MapBuilder MapBuilder()
        {
            return new MapBuilder();
        }

        public static CreatureBuilder CreatureBuilder(CreatureMetaData template=default)
        {
            return new CreatureBuilder(template);
        }
        
        public static StructureBuilder StructureBuilder(StructureMetaData template=default)
        {
            return new StructureBuilder(template);
        }
         */

        
    }
}
