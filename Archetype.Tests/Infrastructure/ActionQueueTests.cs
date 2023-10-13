using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.Implementation;
using FluentAssertions;
using NSubstitute;

namespace Archetype.Tests.Infrastructure;

[TestFixture]
public class ActionQueueTests
{
    private readonly IEventBus _eventBus = Substitute.For<IEventBus>();
    private readonly IRules _rules = Substitute.For<IRules>();
    private ActionQueue _sut = null!;

    [SetUp]
    public void SetUp()
    {
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
        definition.Name.Returns("TestKeyword");
        definition.Resolve(default!, default!).ReturnsForAnyArgs(returnEvent);
        
        var resolutionContext = Substitute.For<IResolutionContext>();
        
        var keywordInstance = Substitute.For<IKeywordInstance>();
        keywordInstance.Keyword.Returns("TestKeyword");
        keywordInstance.Operands.Returns(ArraySegment<KeywordOperand>.Empty);
        keywordInstance.Targets.Returns(ArraySegment<KeywordTarget>.Empty);
        
        var resolutionFrame = Substitute.For<IResolutionFrame>();
        resolutionFrame.Effects.Returns(new List<IKeywordInstance> { keywordInstance });
        resolutionFrame.Context.Returns(resolutionContext);
        
        _rules.GetDefinition("TestKeyword").Returns(definition);
        _sut.Push(resolutionFrame);

        var result = _sut.ResolveNextKeyword();

        result.Should().NotBeNull();
        result.Should().Be(returnEvent);
        
        definition.Resolve(resolutionContext, Arg.Is<EffectPayload>( e => e.Keyword == "TestKeyword")).Received(1);
    }
    
    [Test]
    public void ResolveNextKeyword_CompositeDefinition_ReturnsEvent()
    {
        var primitiveDefinition = Substitute.For<IEffectPrimitiveDefinition>();
        var returnEvent = Substitute.For<IEvent>();
        primitiveDefinition.Name.Returns("PrimitiveTestKeyword");
        primitiveDefinition.Resolve(default!, default!).ReturnsForAnyArgs(returnEvent);
        
        var primitiveKeywordInstance = Substitute.For<IKeywordInstance>();
        primitiveKeywordInstance.Keyword.Returns("PrimitiveTestKeyword");
        primitiveKeywordInstance.Operands.Returns(ArraySegment<KeywordOperand>.Empty);
        primitiveKeywordInstance.Targets.Returns(ArraySegment<KeywordTarget>.Empty);
        
        var compositeDefinition = Substitute.For<IEffectCompositeDefinition>();
        var compositionFrame = Substitute.For<IKeywordFrame>();
        compositionFrame.Effects.Returns(new List<IKeywordInstance> { primitiveKeywordInstance });
        compositeDefinition.Name.Returns("CompositeTestKeyword");
        compositeDefinition.Compose(default!, default!).ReturnsForAnyArgs(compositionFrame);
        
        var compositeKeywordInstance = Substitute.For<IKeywordInstance>();
        compositeKeywordInstance.Keyword.Returns("CompositeTestKeyword");
        compositeKeywordInstance.Operands.Returns(ArraySegment<KeywordOperand>.Empty);
        compositeKeywordInstance.Targets.Returns(ArraySegment<KeywordTarget>.Empty);
        
        var resolutionContext = Substitute.For<IResolutionContext>();
        var resolutionFrame = Substitute.For<IResolutionFrame>();
        resolutionFrame.Effects.Returns(new List<IKeywordInstance> { compositeKeywordInstance });
        resolutionFrame.Context.Returns(resolutionContext);
        
        _rules.GetDefinition("CompositeTestKeyword").Returns(compositeDefinition);
        _rules.GetDefinition("PrimitiveTestKeyword").Returns(primitiveDefinition);
        _sut.Push(resolutionFrame);

        var result = _sut.ResolveNextKeyword();

        result.Should().NotBeNull();
        result.Should().Be(returnEvent);
        
        primitiveDefinition.Resolve(resolutionContext, Arg.Is<EffectPayload>( e => e.Keyword == "PrimitiveTestKeyword")).Received(1);
    }
}
