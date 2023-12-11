using System.Reflection;
using Archetype.Framework.Core.Structure;
using Archetype.Framework.DependencyInjection;
using Archetype.Framework.Design;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Framework.Extensions;

public static class ArchetypeExtensions
{
    private static ServiceProvider _serviceProvider = null!;
    
    // TODO: Use the configure function to add the game state and protoProvider (usually a set parser)
    
    public static IGameRoot InitArchetype<TBootstrapper, TGameState>(TBootstrapper bootstrapper)
        where TGameState : class, IGameState
        where TBootstrapper : class, IBootstrapper
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()))
            .AddSingleton<IGameRoot, GameRoot>()
            .AddSingleton<IInfrastructure, Infrastructure>()
            .AddSingleton<IEventBus, EventBus>()
            .AddSingleton<IEventHistory, EventBus>()
            .AddSingleton<IActionQueue, ActionQueue>()
            .AddSingleton<IGameLoop, GameLoop>()
            .AddSingleton<IGameActionHandler, GameActionHandler>()
            .AddSingleton<IMetaGameState, MetaGameState>()
            .AddSingleton<IProtoData, ProtoData>()
            .AddFactoryProtoCardBuilderFactory()
            .AddSingleton<IBootstrapper, TBootstrapper>()
            .AddSingleton<IGameState, TGameState>();
        
        _serviceProvider = serviceCollection.BuildServiceProvider();
        
        var protoData = _serviceProvider.GetRequiredService<IProtoData>();
        var rules = _serviceProvider.GetRequiredService<IRules>();
        bootstrapper.Bootstrap(protoData, rules);
        
        return _serviceProvider.GetRequiredService<IGameRoot>();
    }
    
    private static IServiceCollection AddFactoryProtoCardBuilderFactory(this IServiceCollection serviceProvider)
    {
        serviceProvider.AddTransient<ProtoCardBuilder>();
        serviceProvider.AddSingleton<IProtoCardBuilderFactory, ProtoCardBuilderFactory>();
        
        return serviceProvider;
    }
}