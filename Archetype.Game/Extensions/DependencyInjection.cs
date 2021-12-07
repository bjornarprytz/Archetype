
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Game.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddArchetypeGameState(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddSingleton<IGameState, GameState>()
                .AddSingleton<IPlayer, Player>()
                .AddSingleton<IMap, Map>()
                .AddSingleton<IHistoryReader, IHistoryWriter, History>();
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
        
        
    }
}