
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Game.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddArchetypeGameState(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<IGameState, GameState>()
                .AddSingleton<IPlayer, Player>()
                .AddSingleton<IMap, Map>()
                ;

            return serviceCollection;
        }
        
        
        
        
        
    }
}