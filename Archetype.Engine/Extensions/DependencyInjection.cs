using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Engine.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddArchetypeEngine(this IServiceCollection serviceCollection)
    {
        return serviceCollection
                .AddSingleton<IContextResolver, ContextResolver>()
                .AddSingleton<IContextFactory<ICardPlayArgs>, CardContextFactory>()
            ;
    }
}