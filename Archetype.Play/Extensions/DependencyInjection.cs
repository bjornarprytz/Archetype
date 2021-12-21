using Archetype.Play.Context;
using Archetype.Play.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Play.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddPlayContext(this IServiceCollection services)
    {
        services
            .AddFactory<ITurnContext, TurnContext>()
            .AddFactory<IGameContext, GameContext>()
            .AddFactory<ISetupContext, SetupContext>()
            .AddFactory<PlayCardContext, PlayCardContext>()
            .AddFactory<IDeckBuilderContext, DeckBuilderContext>()
            .AddSingleton<IPlayCardContextFactory, PlayCardContextFactory>()
            ;
        
        
        return services;
    }

    private static IServiceCollection AddFactory<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddTransient<TService, TImplementation>();
        services.AddSingleton<Func<TService>>(x => () => x.GetService<TService>()!);
        services.AddSingleton<IFactory<TService>, Factory<TService>>();

        return services;
    }
}