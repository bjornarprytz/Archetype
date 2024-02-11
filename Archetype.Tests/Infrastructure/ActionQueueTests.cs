using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Core.Structure;
using Archetype.Framework.Design;
using Archetype.Framework.State;
using Archetype.Tests.MockUtilities;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Archetype.Tests.Infrastructure;

[TestFixture]
public class ActionQueueTests
{
    private IResolutionContext _context = default!;
    private IEventBus _eventBus = default!;
    private IRules _rules = default!;
    private IEffectDefinition _primitiveDefinition = default!;
    private IEffectDefinition _compositeDefinition = default!;
    private IEffectDefinition _promptDefinition = default!;
    private ICostDefinition _costDefinition = default!;
    private ICostDefinition _compositeCostDefinition = default!;

    private ActionQueue _sut = null!;

    private IEffectResult _primitiveEffectResult = null!;
    private IKeywordFrame _compositeEffectResult = null!;
    private IPromptDescription _promptResult = null!;
    private IEffectResult _costResult = null!;
    private IKeywordFrame _compositeCostResult = null!;

    [SetUp]
    public void SetUp()
    {
        _context = Substitute.For<IResolutionContext>();
        _rules = Substitute.For<IRules>();

        _primitiveEffectResult = Substitute.For<IEffectResult>();
        _compositeEffectResult = Substitute.For<IKeywordFrame>();
        _promptResult = Substitute.For<IPromptDescription>();
        _costResult = Substitute.For<IEffectResult>();
        _compositeCostResult = Substitute.For<IKeywordFrame>();

        _primitiveDefinition = Setup.EffectDefinition("PrimitiveTestKeyword", _primitiveEffectResult);
        _rules.GetDefinition("PrimitiveTestKeyword").Returns(_primitiveDefinition);

        _compositeDefinition = Setup.EffectDefinition("CompositeTestKeyword", _compositeEffectResult);
        _rules.GetDefinition("CompositeTestKeyword").Returns(_compositeDefinition);

        _promptDefinition = Setup.EffectDefinition("PromptTestKeyword", _promptResult);
        _rules.GetDefinition("PromptTestKeyword").Returns(_promptDefinition);

        _costDefinition = Setup.CostDefinition("CostTestKeyword", CostType.Resource, _costResult);
        _rules.GetDefinition("CostTestKeyword").Returns(_costDefinition);

        _compositeCostDefinition = Setup.CostDefinition("CompositeCostTestKeyword", CostType.Resource, _compositeCostResult);
        _rules.GetDefinition("CompositeCostTestKeyword").Returns(_compositeCostDefinition);

        _context.MetaGameState.Rules.Returns(_rules);
        _context.Events.Returns(new List<IEvent>());

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
    public void ResolveNextKeyword_WhenResolutionFrameContainsOnePrimitiveEffect_ReturnsEffectResult()
    {
        var primitiveKeywordInstance = Setup.KeywordInstance("PrimitiveTestKeyword");
        var resolutionFrame = Setup.ResolutionFrame(_context, primitiveKeywordInstance);

        _ = _sut.Push(resolutionFrame, Setup.NoCost());

        var result = _sut.ResolveNextKeyword();

        _primitiveDefinition.Received().Resolve(_context, primitiveKeywordInstance);
        result.Should().Be(_primitiveEffectResult);
    }

    [Test]
    public void ResolveNextKeyword_WhenResolutionFrameContainsOneCompositeEffect_ReturnsInnerEffectResult()
    {
        var primitiveKeywordInstance = Setup.KeywordInstance("PrimitiveTestKeyword");
        var compositeKeywordInstance = Setup.KeywordInstance("CompositeTestKeyword");
        _compositeEffectResult.Effects.Returns(new[] { primitiveKeywordInstance });

        var resolutionFrame = Setup.ResolutionFrame(_context, compositeKeywordInstance);

        _ = _sut.Push(resolutionFrame, Setup.NoCost());

        var result = _sut.ResolveNextKeyword();

        _compositeDefinition.Received().Resolve(_context, compositeKeywordInstance);
        _primitiveDefinition.Received().Resolve(_context, primitiveKeywordInstance);
        result.Should().Be(_primitiveEffectResult);
    }

    [Test]
    public void ResolveNextKeyword_WhenLastKeywordInstanceResolves_PublishesActionBlockEventToEventBus()
    {
        var keywordInstance1 = Setup.KeywordInstance("PrimitiveTestKeyword");
        var keywordInstance2 = Setup.KeywordInstance("PrimitiveTestKeyword");

        var compositeKeywordInstance = Setup.KeywordInstance("CompositeTestKeyword");
        _compositeEffectResult.Effects.Returns(new[] { keywordInstance1, keywordInstance2 });

        var resolutionFrame = Setup.ResolutionFrame(_context, compositeKeywordInstance);

        _ = _sut.Push(resolutionFrame, Setup.NoCost());

        _ = _sut.ResolveNextKeyword();
        _ = _sut.ResolveNextKeyword();

        _eventBus.Received(1).Publish(Arg.Is<ActionBlockEvent>(e =>
            e.Parent == null
            && e.Source == _context.Source
            && e.Targets == _context.Targets
            && e.Payments == _context.Payments
            && e.ComputedValues == _context.ComputedValues
            && e.PromptResponses == _context.PromptResponses
            && e.Children.Count == 1
            && e.Children[0] is EffectEvent && ((EffectEvent)e.Children[0]).KeywordInstance == compositeKeywordInstance

            && e.Children[0].Children.Count == 2

            && e.Children[0].Children[0] is EffectEvent
            && ((EffectEvent)e.Children[0].Children[0]).KeywordInstance == keywordInstance1
            && ((EffectEvent)e.Children[0].Children[0]).Result == _primitiveEffectResult

            && e.Children[0].Children[1] is EffectEvent
            && ((EffectEvent)e.Children[0].Children[1]).KeywordInstance == keywordInstance2
            && ((EffectEvent)e.Children[0].Children[1]).Result == _primitiveEffectResult
        ));
    }

    [Test]
    public void Push_WhenDryRunFails_ReturnsFailureResult()
    {
        var paymentContext = Setup.PaymentContext(_context, CostType.Resource, Setup.KeywordInstance("CostTestKeyword"), Substitute.For<IAtom>());
        _costDefinition.DryRun(default!, default!, default!).ReturnsForAnyArgs(new FailureResult("Fail reason"));

        var result = _sut.Push(Substitute.For<IResolutionFrame>(), paymentContext);

        _costDefinition.DidNotReceiveWithAnyArgs().Pay(default!, default!, default!);
        result.Should().BeOfType<FailureResult>();
        result.As<FailureResult>().Message.Should().Be("Fail reason");
    }

    [Test]
    public void Push_WhenPaymentSucceeds_ReturnsSuccess()
    {
        var payment = new[] { Substitute.For<IAtom>() };

        var paymentContext = Setup.PaymentContext(_context, CostType.Resource, Setup.KeywordInstance("CostTestKeyword"), payment);
        _costDefinition.DryRun(default!, default!, default!).ReturnsForAnyArgs(EffectResult.Resolved);
        _costDefinition.Pay(default!, default!, default!).ReturnsForAnyArgs(EffectResult.Resolved);

        var result = _sut.Push(Substitute.For<IResolutionFrame>(), paymentContext);

        result.Should().Be(EffectResult.Resolved);
        var cost = paymentContext.Costs.Single();

        _costDefinition.Received().Pay(_context, cost, payment);
        _costDefinition.Received().Pay(_context, cost, payment);
    }

    [Test]
    public void Push_WhenPaymentSucceeds_PublishesPaymentEventToEventBus()
    {
        var payment = new[] { Substitute.For<IAtom>() };
        var nestedKeywordInstance1 = Setup.KeywordInstance("PrimitiveTestKeyword");
        var nestedKeywordInstance2 = Setup.KeywordInstance("PrimitiveTestKeyword");
        var compositeKeywordInstance = Setup.KeywordInstance("CompositeCostTestKeyword");
        _compositeCostResult.Effects.Returns(new[] { nestedKeywordInstance1, nestedKeywordInstance2 });

        var paymentContext = Setup.PaymentContext(_context, CostType.Resource, compositeKeywordInstance, payment);
        _costDefinition.DryRun(default!, default!, default!).ReturnsForAnyArgs(EffectResult.Resolved);
        _costDefinition.Pay(default!, default!, default!).ReturnsForAnyArgs(EffectResult.Resolved);

        _ = _sut.Push(Substitute.For<IResolutionFrame>(), paymentContext);

        _eventBus.Received(1).Publish(Arg.Is<PaymentEvent>(e =>
            e.Parent == null
            && e.Source == _context.Source
            && e.Children.Count == 1
            && e.Children[0] is EffectEvent && ((EffectEvent)e.Children[0]).KeywordInstance == compositeKeywordInstance

            && e.Children[0].Children.Count == 2

            && e.Children[0].Children[0] is EffectEvent
            && ((EffectEvent)e.Children[0].Children[0]).KeywordInstance == nestedKeywordInstance1
            && ((EffectEvent)e.Children[0].Children[0]).Result == _primitiveEffectResult

            && e.Children[0].Children[1] is EffectEvent
            && ((EffectEvent)e.Children[0].Children[1]).KeywordInstance == nestedKeywordInstance2
            && ((EffectEvent)e.Children[0].Children[1]).Result == _primitiveEffectResult
        ));
    }

    [Test]
    public void ResolvePrompt_WhenPromptIsAlreadyAnswered_ReturnsFailureResult()
    {
        var promptId = Guid.NewGuid();
        var prompt = Substitute.For<PromptResponse>();
        prompt.IsPending.Returns(false);

        _context.PromptResponses.Returns(new Dictionary<Guid, PromptResponse>()
        {
            { promptId, prompt }
        });

        var promptContext = Setup.PromptContext(promptId, Substitute.For<IAtom>());

        var result = _sut.ResolvePrompt(promptContext);

        result.Should().BeOfType<FailureResult>();
        result.As<FailureResult>().Message.Should().Be("Prompt already answered");
    }

    [Test]
    public void ResolvePrompt_OutsideResolutionFrame_ThrowsInvalidOperationException()
    {
        var promptId = Guid.NewGuid();

        var promptContext = Setup.PromptContext(promptId, Substitute.For<IAtom>());

        Action act = () => _ = _sut.ResolvePrompt(promptContext);

        act.Should().Throw<InvalidOperationException>().WithMessage("No frame/context to resolve prompt");
    }

    [Test]
    public void ResolveNextKeyword_WhenPromptIsPending_ReturnsFailure()
    {
        var compositeKeywordInstance = Setup.KeywordInstance("CompositeTestKeyword");
        var promptKeywordInstance = Setup.KeywordInstance("PromptTestKeyword");
        var primitiveKeywordInstance = Setup.KeywordInstance("PrimitiveTestKeyword");
        _compositeEffectResult.Effects.Returns(new[] { promptKeywordInstance, primitiveKeywordInstance });

        var resolutionFrame = Setup.ResolutionFrame(_context, compositeKeywordInstance);

        _ = _sut.Push(resolutionFrame, Setup.NoCost());
        _ = _sut.ResolveNextKeyword();

        var result = _sut.ResolveNextKeyword();

        result.Should().BeOfType<FailureResult>();
        result.As<FailureResult>().Message.Should().Be("Prompt pending");
    }


    [Test]
    public void ResolveNextKeyword_WhenMultipleResolutionFrames_TheyAreResolvedInOrder()
    {
        var keywordInstance1 = Setup.KeywordInstance("PrimitiveTestKeyword");

        var keywordResult2 = Substitute.For<IEffectResult>();
        var primitiveDefinition2 = Setup.EffectDefinition("PrimitiveTestKeyword2", keywordResult2);
        _rules.GetDefinition("PrimitiveTestKeyword2").Returns(primitiveDefinition2);
        var keywordInstance2 = Setup.KeywordInstance("PrimitiveTestKeyword2");
        var context2 = Substitute.For<IResolutionContext>();
        context2.MetaGameState.Rules.Returns(_rules);

        var resolutionFrame1 = Setup.ResolutionFrame(_context, keywordInstance1);
        var resolutionFrame2 = Setup.ResolutionFrame(context2, keywordInstance2);

        _ = _sut.Push(resolutionFrame1, Setup.NoCost());
        _ = _sut.Push(resolutionFrame2, Setup.NoCost());

        _ = _sut.ResolveNextKeyword();
        _primitiveDefinition.Received().Resolve(_context, keywordInstance1);
        _ = _sut.ResolveNextKeyword();
        primitiveDefinition2.Received().Resolve(context2, keywordInstance2);
    }

    [Test]
    public void ResolvePrompt_WhenResolutionFrameIsNotNullAndPromptIsNotAnswered_ReturnsSuccess()
    {
        var promptId = Guid.NewGuid();
        _promptResult.PromptId.Returns(promptId);

        var compositeKeywordInstance = Setup.KeywordInstance("CompositeTestKeyword");
        var promptKeywordInstance = Setup.KeywordInstance("PromptTestKeyword");
        var primitiveKeywordInstance = Setup.KeywordInstance("PrimitiveTestKeyword");
        _compositeEffectResult.Effects.Returns(new[] { promptKeywordInstance, primitiveKeywordInstance });

        var resolutionFrame = Setup.ResolutionFrame(_context, compositeKeywordInstance);

        _ = _sut.Push(resolutionFrame, Setup.NoCost());
        /* IPromptDescription */
        _ = _sut.ResolveNextKeyword();

        var promptContext = Setup.PromptContext(promptId);
        var result = _sut.ResolvePrompt(promptContext);

        result.Should().Be(EffectResult.Resolved);
    }
}
