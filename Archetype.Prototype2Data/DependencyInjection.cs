

using Archetype.Prototype2Data.GameGraph;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Prototype2Data;

public static class DependencyInjection
{
    public static IServiceCollection AddPrototype2(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<IGameStateView>(Generator.Create())
            .AddSingleton<IGameView, GameView>();
    }
}