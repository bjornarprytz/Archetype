using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Prototype1Data;

public static class DependencyInjection
{
    public static IServiceCollection AddPrototype1(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<IGameState>(Generator.Create())
            .AddSingleton<IGameView, GameView>();
    }
}