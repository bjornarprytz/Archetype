using System.Reflection;
using Archetype.Framework.Core.Structure;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Framework.Extensions;

public static class ArchetypeExtensions
{
    public static IServiceCollection AddArchetype(this IServiceCollection serviceProvider)
    {
        serviceProvider
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()))
            .AddSingleton<IGameRoot, GameRoot>()
            .AddSingleton<IInfrastructure, Infrastructure>()
            .AddSingleton<IEventBus, EventBus>()
            .AddSingleton<IEventHistory, EventBus>()
            .AddSingleton<IActionQueue, ActionQueue>()
            .AddSingleton<IGameLoop, GameLoop>()
            .AddSingleton<IGameActionHandler, GameActionHandler>()
            .AddSingleton<IMetaGameState, MetaGameState>();

        return serviceProvider;
    }
}