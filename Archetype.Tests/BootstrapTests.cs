using Archetype.BasicRules;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;
using NSubstitute;

namespace Archetype.Tests;

[TestFixture]
public class BootstrapTests
{
    
    [Test]
    public void Bootstrap()
    {
        var rules = Substitute.For<IRules>();
        var protoCards = Substitute.For<IProtoCards>();
        var gameState = Substitute.For<IGameState>();
        
        var gameRoot = ArchetypeBootstrapper.Bootstrap(
            () => rules,
            () => protoCards,
            () => gameState
        );
        
        Assert.That(gameRoot, Is.Not.Null);
        Assert.That(gameRoot.MetaGameState, Is.Not.Null);
        Assert.That(gameRoot.GameState, Is.Not.Null);
        Assert.That(gameRoot.Infrastructure, Is.Not.Null);
    }
}