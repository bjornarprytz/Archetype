using Archetype.Framework;
using Archetype.Framework.GameLoop;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Archetype.Tests.Integration;

public class GameTests
{
    [Fact]
    public void TestGame()
    {
        // Arrange
        var initialState = Substitute.For<IGameState>();
        var rules = Substitute.For<IRules>();
        
        // Act
        var game = Bootstrap.StartGame(initialState, rules);
        
        // Assert
        true.Should().BeFalse(); // TODO: What else?
    }
}