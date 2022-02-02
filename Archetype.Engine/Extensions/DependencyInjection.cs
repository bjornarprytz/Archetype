using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Engine.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddEngine(this IServiceCollection serviceCollection)
    {
        return serviceCollection
                .AddSingleton<IContextResolver, ContextResolver>()
                .AddSingleton<IContextFactory<ICardPlayArgs>, CardContextFactory>()
            ;
    }
}