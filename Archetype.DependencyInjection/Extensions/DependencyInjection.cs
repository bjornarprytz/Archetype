using Archetype.Builder.Extensions;
using Archetype.Core.Extensions;
using Archetype.Design.Extensions;
using Archetype.Engine.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddArchetype(this IServiceCollection serviceCollection)
    {
        return serviceCollection
                .AddCore()
                .AddBuilders()
                .AddEngine()
                .AddDesign()
            ;
    }

    private static IServiceCollection AddSingleton<I1, I2, T>(this IServiceCollection serviceCollection)
        where T : class, I1, I2
        where I1 : class
        where I2 : class
    {
        return serviceCollection
            .AddSingleton<T>()
            .AddSingleton<I1, T>(s => s.GetService<T>())
            .AddSingleton<I2, T>(s => s.GetService<T>());
    }
        
    private static IServiceCollection AddSingleton<I1, I2, I3, T>(this IServiceCollection serviceCollection)
        where T : class, I1, I2, I3
        where I1 : class
        where I2 : class
        where I3 : class
    {
        return serviceCollection
            .AddSingleton<T>()
            .AddSingleton<I1, T>(s => s.GetService<T>())
            .AddSingleton<I2, T>(s => s.GetService<T>())
            .AddSingleton<I3, T>(s => s.GetService<T>());
    }
}