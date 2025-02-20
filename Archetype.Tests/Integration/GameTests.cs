using Archetype.Framework;
using Archetype.Framework.Events;
using Archetype.Framework.GameLoop;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Archetype.Tests.Integration;

public class GameTests
{
    private readonly IRules _rules = Substitute.For<IRules>();
    private readonly IScope _rootScope = Substitute.For<IScope>();
    private readonly IGameState _state = Substitute.For<IGameState>();

    private readonly IGameRoot _game;
    public GameTests()
    {
        _rootScope.CurrentSubScope.Returns(default(IScope));
        _rules.CreateInitialState().Returns(_state);
        _game = Bootstrap.Init(_rules, _rootScope);
    }
    
    [Fact]
    public void Init_InitialConditionsAreCorrect()
    {
        // Arrange (done in constructor)
        // Act (done in constructor)
        // Assert
        _rules.Received(1).CreateInitialState();
        
        _game.RootScope.Should().Be(_rootScope);
        _game.State.Should().Be(_state);
    }
    
    [Fact]
    public void TakeAction_ResolvesActionOnTheEdgeScope()
    {
        // Arrange
        var actionArgs = Substitute.For<IActionArgs>();
        var subScope = Substitute.For<IScope>();
        var edgeScope = Substitute.For<IScope>();
        _rootScope.CurrentSubScope.Returns(subScope);
        subScope.CurrentSubScope.Returns(edgeScope);
        edgeScope.CurrentSubScope.Returns(default(IScope));
        
        // Act
        _game.TakeAction(actionArgs);
        
        // Assert
        _rules.Received(1).ResolveAction(_state, edgeScope, actionArgs);
    }
    
    [Fact]
    public void TakeAction_ReturnsAllEvents()
    {
        // Arrange
        var events = new List<IEvent>()
        {
            Substitute.For<IEvent>(),
            Substitute.For<IEvent>(),
        };

        _rules.ResolveAction(default!, default!, default!).ReturnsForAnyArgs(events);
        
        // Act
        var returnedEvents = _game.TakeAction(Substitute.For<IActionArgs>());
        
        // Assert
        returnedEvents.Should().BeEquivalentTo(events);
    }
    
    [Fact]
    public void TakeAction_ScopeLoops_ThrowsException()
    {
        // Arrange
        var loopScope = Substitute.For<IScope>();
        _rootScope.CurrentSubScope.Returns(loopScope);
        loopScope.CurrentSubScope.Returns(_rootScope);
        
        // Act
        Action act = () => _game.TakeAction(Substitute.For<IActionArgs>());
        
        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}