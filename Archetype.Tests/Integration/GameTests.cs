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
        var rules = Substitute.For<IRules>();
        var cardPool = Substitute.For<ICardPool>();
        
        // Act
        var game = Bootstrap.StartGame(rules, cardPool);
        
        // Assert
        true.Should().BeFalse(); // TODO: What else?
    }
}