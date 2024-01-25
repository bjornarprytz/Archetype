using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Core.Structure;
using Archetype.Framework.Design;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;

namespace Archetype.Tests.Infrastructure;

[TestFixture]
public class ActionQueueTests
{
    private IEventBus _eventBus;
    private IRules _rules;
    private IEffectPrimitiveDefinition _primitiveDefinition;
    private IEffectCompositeDefinition _compositeDefinition;
    private ActionQueue _sut = null!;
    

    [SetUp]
    public void SetUp()
    {
        _rules = Substitute.For<IRules>();
        _primitiveDefinition = Substitute.For<IEffectPrimitiveDefinition>();
        _compositeDefinition = Substitute.For<IEffectCompositeDefinition>();
        _primitiveDefinition.Keyword.Returns("PrimitiveTestKeyword");
        _compositeDefinition.Keyword.Returns("CompositeTestKeyword");
        _rules.GetDefinition("CompositeTestKeyword").Returns(_compositeDefinition);
        _rules.GetDefinition("PrimitiveTestKeyword").Returns(_primitiveDefinition);
        
        _eventBus = Substitute.For<IEventBus>();
        
        _sut = new ActionQueue(_eventBus, _rules);
    }

    [Test]
    public void ResolveNextKeyword_WhenKeywordStackIsEmpty_ReturnsNull()
    {
        var result = _sut.ResolveNextKeyword();
        
        result.Should().BeNull();
    }

    [Test]
    public void ResolveNextKeyword_PrimitiveDefinition_ReturnsEvent()
    {
        var definition = Substitute.For<IEffectPrimitiveDefinition>();
        var returnEvent = Substitute.For<IEvent>();
        definition.Keyword.Returns("TestKeyword");
        definition.Resolve(default!, default!).ReturnsForAnyArgs(returnEvent);
        
        var resolutionContext = Substitute.For<IResolutionContext>();
        
        var keywordInstance = Substitute.For<IKeywordInstance>();
        keywordInstance.Keyword.Returns("TestKeyword");
        keywordInstance.Operands.Returns(ArraySegment<KeywordOperand>.Empty);
        
        var resolutionFrame = Substitute.For<IResolutionFrame>();
        resolutionFrame.Effects.Returns(new List<IKeywordInstance> { keywordInstance });
        resolutionFrame.Context.Returns(resolutionContext);
        
        _rules.GetDefinition("TestKeyword").Returns(definition);
        _sut.Push(resolutionFrame);

        var result = _sut.ResolveNextKeyword();

        result.Should().NotBeNull();
        result.Should().Be(returnEvent);
        
        definition.Resolve(resolutionContext, Arg.Is<EffectPayload>( e => e.EffectId == "TestKeyword")).Received(1);
    }
    
    [Test]
    public void ResolveNextKeyword_CompositeDefinition_ReturnsEvent()
    {
        var returnEvent = Substitute.For<IEvent>();
        _primitiveDefinition.Resolve(default!, default!).ReturnsForAnyArgs(returnEvent);
        
        var primitiveKeywordInstance = Substitute.For<IKeywordInstance>();
        primitiveKeywordInstance.Keyword.Returns("PrimitiveTestKeyword");
        
        var keywordFrame = Substitute.For<IKeywordFrame>();
        keywordFrame.Effects.Returns(new List<IKeywordInstance> { primitiveKeywordInstance });
        _compositeDefinition.Compose(default!, default!).ReturnsForAnyArgs(keywordFrame);
        
        var compositeKeywordInstance = Substitute.For<IKeywordInstance>();
        compositeKeywordInstance.Keyword.Returns("CompositeTestKeyword");
        
        var resolutionContext = Substitute.For<IResolutionContext>();
        var resolutionFrame = Substitute.For<IResolutionFrame>();
        resolutionFrame.Effects.Returns(new List<IKeywordInstance> { compositeKeywordInstance });
        resolutionFrame.Context.Returns(resolutionContext);
        
        _sut.Push(resolutionFrame);

        var result = _sut.ResolveNextKeyword();

        result.Should().NotBeNull();
        result.Should().Be(returnEvent);
        
        _primitiveDefinition.Resolve(resolutionContext, Arg.Is<EffectPayload>( e => e.EffectId == "PrimitiveTestKeyword")).Received(1);
    }
    
    [Test]
    public void ResolveNextKeyword_CompositeDefinition_PublishesActionBlockEvent()
    {
        var otherCompositeDefinition = Substitute.For<IEffectCompositeDefinition>();
        otherCompositeDefinition.Keyword.Returns("OtherCompositeTestKeyword");
        _rules.GetDefinition("OtherCompositeTestKeyword").Returns(otherCompositeDefinition);
        
        var primitiveKeywordInstance1 = Substitute.For<IKeywordInstance>();
        primitiveKeywordInstance1.Keyword.Returns("PrimitiveTestKeyword");
        primitiveKeywordInstance1.Id.Returns(Guid.NewGuid());
        var primitiveKeywordInstance2 = Substitute.For<IKeywordInstance>();
        primitiveKeywordInstance2.Keyword.Returns("PrimitiveTestKeyword");
        primitiveKeywordInstance2.Id.Returns(Guid.NewGuid());
        var primitiveKeywordInstance3 = Substitute.For<IKeywordInstance>();
        primitiveKeywordInstance3.Keyword.Returns("PrimitiveTestKeyword");
        primitiveKeywordInstance3.Id.Returns(Guid.NewGuid());
        var compositeKeywordInstance = Substitute.For<IKeywordInstance>();
        compositeKeywordInstance.Keyword.Returns("CompositeTestKeyword");
        var otherCompositeKeywordInstance = Substitute.For<IKeywordInstance>();
        otherCompositeKeywordInstance.Keyword.Returns("OtherCompositeTestKeyword");
        
        
        
        var primitiveEvent = Substitute.For<IEvent>();
        _primitiveDefinition.Resolve(default!, default!).ReturnsForAnyArgs(primitiveEvent);
        var compositeEvent = Substitute.For<IEvent>();
        compositeEvent.Children.Returns(new List<IEvent>());
        var otherCompositeEvent = Substitute.For<IEvent>();
        otherCompositeEvent.Children.Returns(new List<IEvent>());
        
        var keywordFrame = Substitute.For<IKeywordFrame>();
        keywordFrame.Event.Returns(compositeEvent);
        keywordFrame.Effects.Returns(new List<IKeywordInstance> { primitiveKeywordInstance1 });
        _compositeDefinition.Compose(default!, default!).ReturnsForAnyArgs(keywordFrame);
        
        var otherKeywordFrame = Substitute.For<IKeywordFrame>();
        otherKeywordFrame.Event.Returns(otherCompositeEvent);
        otherKeywordFrame.Effects.Returns(new List<IKeywordInstance> { primitiveKeywordInstance2 });
        otherCompositeDefinition.Compose(default!, default!).ReturnsForAnyArgs(otherKeywordFrame);
        
        var resolutionContext = Substitute.For<IResolutionContext>();
        var resolutionFrame = Substitute.For<IResolutionFrame>();
        var source = Substitute.For<IAtom>();
        resolutionContext.Source.Returns(source);
        resolutionContext.Events.Returns(new List<IEvent>());
        resolutionFrame.Effects.Returns(new List<IKeywordInstance> { compositeKeywordInstance, otherCompositeKeywordInstance, primitiveKeywordInstance3 });
        resolutionFrame.Context.Returns(resolutionContext);
        
        _sut.Push(resolutionFrame);

        while(_sut.ResolveNextKeyword() != null) { };
        
        _eventBus.Received(1).Publish(Arg.Is<IActionBlockEvent>(e => e.Source == resolutionContext.Source 
                                                                     && e.Targets == resolutionContext.Targets 
                                                                     && e.Payments == resolutionContext.Payments 
                                                                     && e.ComputedValues == resolutionContext.ComputedValues 
                                                                     && e.PromptResponses == resolutionContext.PromptResponses
                                                                     && e.Children.SequenceEqual(new []{ compositeEvent, otherCompositeEvent, primitiveEvent })
        ));
        
        compositeEvent.Children.Should().Contain(primitiveEvent);
        otherCompositeEvent.Children.Should().Contain(primitiveEvent);
    }
}
