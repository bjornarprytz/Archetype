using System;
using Archetype.Builder.Builders;
using Archetype.Builder.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Builder.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddBuilders(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddFactory<ICardBuilder, CardBuilder>()
            .AddFactory<IStructureBuilder, StructureBuilder>()
            .AddFactory<ICreatureBuilder, CreatureBuilder>()
            .AddFactory<IMapBuilder, MapBuilder>()
            .AddFactory<ISetBuilder, SetBuilder>();

        return serviceCollection;
    }
        
    private static IServiceCollection AddFactory<TService, TImplementation>(this IServiceCollection services) 
        where TService : class
        where TImplementation : class, TService
    {
        services.AddTransient<TService, TImplementation>();
        services.AddSingleton<Func<TService>>(x => x.GetService<TService>);
        services.AddSingleton<IFactory<TService>, Factory<TService>>();

        return services;
    }
}