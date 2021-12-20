using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Archetype.Builder.Builders;
using Archetype.Builder.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Builder.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBuilders(this IServiceCollection serviceCollection)
        {
            // TODO: Scan assembly for implementations instead
            
            serviceCollection.AddSingleton<IBuilderFactory, BuilderFactory>();

            serviceCollection.AddTransient<ICardBuilder, CardBuilder>();
            serviceCollection.AddTransient<IStructureBuilder, StructureBuilder>();
            serviceCollection.AddTransient<ICreatureBuilder, CreatureBuilder>();
            serviceCollection.AddTransient<ICardEffectBuilder, CardEffectBuilder>();
            serviceCollection.AddTransient(typeof(ICardEffectBuilder<>), typeof(CardEffectBuilder<>));
            serviceCollection.AddTransient<IMapBuilder, MapBuilder>();
            serviceCollection.AddTransient<INodeBuilder, NodeBuilder>();
            serviceCollection.AddTransient<ISetBuilder, SetBuilder>();
            
            return serviceCollection;
        }
    }
}