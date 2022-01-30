using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.View.Atoms;
using Archetype.View.Infrastructure;
using Archetype.View.Proto;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Game.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddArchetype(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                    .AddSingleton<IProtoPool, IProtoPoolFront, ProtoPool>()
                    .AddSingleton<IGameState, IGameStateFront, GameState>()
                    .AddSingleton<IPlayer, IPlayerFront, Player>()
                    .AddSingleton<IPlayerData, IPlayerDataFront, PlayerData>()
                    .AddSingleton<IMap, IMapFront, Map>()
                    .AddSingleton<IHistoryReader, IHistoryWriter, IHistoryEmitter, History>()
                    .AddSingleton<IInstanceFactory, IInstanceFinder, InstanceManager>()
                    
                    .AddSingleton<IContextResolver, ContextResolver>()
                    
                    .AddSingleton<IContextFactory<ICardPlayArgs>, CardContextFactory>()
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
}