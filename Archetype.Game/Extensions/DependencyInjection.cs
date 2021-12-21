
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Context.Phases;
using Archetype.Game.Payloads.Context.Trigger;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Proto;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Game.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddArchetype(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                    .AddSingleton<IProtoPool, ProtoPool>()
                    .AddSingleton<IGameState, GameState>()
                    .AddSingleton<IPlayer, Player>()
                    .AddSingleton<IPlayerData, PlayerData>()
                    .AddSingleton<IMap, Map>()
                    .AddSingleton<IHistoryReader, IHistoryWriter, History>()
                    .AddSingleton<IInstanceFactory, IInstanceFinder, InstanceManager>()
                    .AddSingleton<ICardResolver, CardResolver>()
                    .AddSingleton(typeof(ITriggerResolver<>), typeof(TriggerResolver<>))
                    
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