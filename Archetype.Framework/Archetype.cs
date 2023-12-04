using System.Reflection;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.Actions;
using Archetype.Framework.Runtime.Implementation;
using Archetype.Framework.Runtime.State;
using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace Archetype.BasicRules;

public static class ArchetypeBootstrapper
{
    private static IServiceProvider _serviceProvider = null!;
    
    public static IGameRoot Bootstrap(
        Func<IRules> rulesFactory,
        Func<IProtoCards> protoCardsFactory,
        Func<IGameState> gameStateFactory
        )
    {
        _serviceProvider = new ServiceCollection()
            .AddSingleton(rulesFactory())
            .AddSingleton(protoCardsFactory())
            .AddSingleton(gameStateFactory())
            .AddMediatR(cfg=>cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()))
            .AddSingleton<IGameRoot, GameRoot>()
            .AddSingleton<IInfrastructure, Infrastructure>()
            .AddSingleton<IEventBus, EventBus>()
            .AddSingleton<IEventHistory, EventBus>()
            .AddSingleton<IActionQueue, ActionQueue>()
            .AddSingleton<IGameLoop, GameLoop>()
            .AddSingleton<IGameActionHandler, GameActionHandler>()
            .AddSingleton<IMetaGameState, MetaGameState>()
            .BuildServiceProvider();

        return _serviceProvider.GetService<IGameRoot>()!;
    }
}