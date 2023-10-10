using Archetype.Framework.Runtime;
using FluentAssertions;

namespace Archetype.Tests.Infrastructure;

[TestFixture]
public class QueueStackTests
{
    [Test]
    public void Enqueue_AddsItemToEndOfStack()
    {
        // Arrange
        var queueStack = new QueueStack<int>();
        queueStack.Enqueue(1);

        // Act
        var item = queueStack.Pop();

        // Assert
        item.Should().Be(1);
    }

    [Test]
    public void Push_AddsItemToTopOfStack()
    {
        // Arrange
        var queueStack = new QueueStack<int>();
        queueStack.Push(1);

        // Act
        var item = queueStack.Pop();

        // Assert
        item.Should().Be(1);
    }

    [Test]
    public void Pop_RemovesAndReturnsTopItem()
    {
        // Arrange
        var queueStack = new QueueStack<int>();
        queueStack.Push(1);
        queueStack.Push(2);
        queueStack.Push(3);

        // Act
        var item1 = queueStack.Pop();
        var item2 = queueStack.Pop();
        var item3 = queueStack.Pop();

        // Assert
        item1.Should().Be(3);
        item2.Should().Be(2);
        item3.Should().Be(1);
    }

    [Test]
    public void Pop_ThrowsInvalidOperationExceptionWhenEmpty()
    {
        // Arrange
        var queueStack = new QueueStack<int>();

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => queueStack.Pop());
    }

    [Test]
    public void Count_ReturnsNumberOfItemsInStack()
    {
        // Arrange
        var queueStack = new QueueStack<int>();
        queueStack.Push(1);
        queueStack.Push(2);

        // Act
        var count = queueStack.Count;

        // Assert
        count.Should().Be(2);
    }

    [Test]
    public void Count_ReturnsZeroWhenStackIsEmpty()
    {
        // Arrange
        var queueStack = new QueueStack<int>();

        // Act
        var count = queueStack.Count;

        // Assert
        count.Should().Be(0);
    }
}
