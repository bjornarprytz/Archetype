using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Context.Phases;
using Archetype.Game.Payloads.Context.Phases.Base;
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
                    .AddSingleton<IHistoryReader, IHistoryWriter, History>()
                    .AddSingleton<IInstanceFactory, IInstanceFinder, InstanceManager>()
                    
                    .AddTransient<IMutationObserver, MutationObserver>()
                    
                    .AddSingleton<IContextResolver, ContextResolver>()
                    
                    .AddSingleton<IContextFactory<ICardPlayArgs>, CardContextFactory>()
                    
                    .AddSingleton<IMovePhaseResolver, MovePhase>()
                    .AddSingleton<ICombatPhaseResolver, CombatPhase>()
                    .AddSingleton<IUpkeepPhaseResolver, UpkeepPhase>()
                    .AddSingleton<ISpawnPhaseResolver, SpawnPhase>()
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
        
        
    }
}