using Archetype.Framework.Core.Structure;
using Archetype.Framework.Design;
using Archetype.Framework.Extensions;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Archetype.Tests;

[TestFixture]
public class DependencyInjectionTests
{
    
    [Test]
    public void AddArchetype()
    {
        var rules = Substitute.For<IRules>();
        var protoCards = Substitute.For<IProtoCards>();
        var gameState = Substitute.For<IGameState>();
        
        var services = new ServiceCollection();
        services.AddArchetype()
            .AddSingleton(rules);
        services.AddSingleton(protoCards);
        services.AddSingleton(gameState);
        
        var provider = services.BuildServiceProvider();
        var gameRoot = provider.GetRequiredService<IGameRoot>();
        var infrastructure = provider.GetRequiredService<IInfrastructure>();
        var eventBus = provider.GetRequiredService<IEventBus>();
        var eventHistory = provider.GetRequiredService<IEventHistory>();
        var actionQueue = provider.GetRequiredService<IActionQueue>();
        var gameLoop = provider.GetRequiredService<IGameLoop>();
        var gameActionHandler = provider.GetRequiredService<IGameActionHandler>();
        var metaGameState = provider.GetRequiredService<IMetaGameState>();
        var mediator = provider.GetRequiredService<IMediator>();

        gameRoot.Should().NotBeNull();
        infrastructure.Should().NotBeNull();
        eventBus.Should().NotBeNull();
        eventHistory.Should().NotBeNull();
        actionQueue.Should().NotBeNull();
        gameLoop.Should().NotBeNull();
        gameActionHandler.Should().NotBeNull();
        metaGameState.Should().NotBeNull();
        mediator.Should().NotBeNull();
    }
}