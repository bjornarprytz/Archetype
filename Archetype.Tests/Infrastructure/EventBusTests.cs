using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Core.Structure;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;

namespace Archetype.Tests.Infrastructure;

[TestFixture]
public class EventBusTests
{
    private EventBus _sut = null!;
    
    [SetUp]
    public void SetUp()
    {
        _sut = new EventBus();
    }
    
    [Test]
    public void Publish_WhenCalled_AddsEventToActionBlockEvents()
    {
        var actionBlockEvent = Substitute.For<IActionBlockEvent>();
        
        _sut.Publish(actionBlockEvent);
        
        _sut.RootEvents.Should().Contain(actionBlockEvent);
    }
    
    [Test]
    public void Publish_WhenCalled_KeywordEventsShouldContainSubEventsInOrder()
    {
        var actionBlockEvent = Substitute.For<IActionBlockEvent>();
        var keywordEvent1 = Substitute.For<IEvent>();
        var keywordEvent2 = Substitute.For<IEvent>();
        var l1Event1 = Substitute.For<IEvent>();
        var l1Event2 = Substitute.For<IEvent>();
        var l2Event1 = Substitute.For<IEvent>();
        var l2Event2 = Substitute.For<IEvent>();
        var l2Event3 = Substitute.For<IEvent>();
        var l3Event1 = Substitute.For<IEvent>();
        actionBlockEvent.Children.Returns(new List<IEvent> { keywordEvent1, keywordEvent2 });
        keywordEvent1.Children.Returns(new List<IEvent> { l1Event1, l1Event2 });
        l1Event1.Children.Returns(new List<IEvent> { l2Event1, l2Event2 });
        l1Event2.Children.Returns(new List<IEvent> { l2Event3 });
        l2Event1.Children.Returns(new List<IEvent> { l3Event1 });
        
        _sut.Publish(actionBlockEvent);
        
        _sut.KeywordEvents.Should().ContainInOrder(l3Event1, l2Event1, l2Event2, l1Event1, l2Event3, l1Event2, keywordEvent1, keywordEvent2);
    }

    [Test]
    public void Subscribe_HandlerIsCalledOnce()
    {
        var actionBlockEvent = Substitute.For<IActionBlockEvent>();
        var keywordEvent1 = Substitute.For<IEvent>();
        actionBlockEvent.Children.Returns(new List<IEvent> { keywordEvent1 });
        var subscriber = Substitute.For<IAtom>();
        
        var handler = Substitute.For<Action<IEvent>>();
        
        _sut.Subscribe(subscriber, handler);
        _sut.Publish(actionBlockEvent);
        
        handler.Received(1).Invoke(keywordEvent1);
    }
    
    [Test]
    public void Subscribe_ThenUnsubscribe_HandlerIsNotCalled()
    {
        
        var actionBlockEvent = Substitute.For<IActionBlockEvent>();
        var keywordEvent1 = Substitute.For<IEvent>();
        actionBlockEvent.Children.Returns(new List<IEvent> { keywordEvent1 });
        var subscriber = Substitute.For<IAtom>();

        var handler = Substitute.For<Action<IEvent>>();
        
        _sut.Subscribe(subscriber, handler);
        _sut.Unsubscribe(subscriber, handler);
        _sut.Publish(actionBlockEvent);
        
        handler.DidNotReceiveWithAnyArgs().Invoke(default!);
    }
}