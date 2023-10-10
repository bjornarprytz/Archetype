using Archetype.BasicRules.Primitives;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.Implementation;
using FluentAssertions;
using NSubstitute.Extensions;

namespace Archetype.Tests.Infrastructure;

using Archetype.Framework.Proto;
using Archetype.Framework.Runtime.Actions;
using Archetype.Framework.Runtime.State;
using NUnit.Framework;
using NSubstitute;
using System;
using System.Collections.Generic;

[TestFixture]
public class GameLoopTests
{
    private readonly IGameRoot _gameRoot = Substitute.For<IGameRoot>();
    private readonly IActionQueue _actionQueue = Substitute.For<IActionQueue>();
    private readonly IReadOnlyList<IPhase> _phases = new List<IPhase> { Substitute.For<IPhase>(), Substitute.For<IPhase>() };
    private GameLoop _sut = default!;

    [SetUp]
    public void SetUp()
    {
        
        _actionQueue.ResolveNextKeyword().Returns(null as IEvent);
        
        _gameRoot.Infrastructure.ActionQueue.Returns(_actionQueue);
        _gameRoot.MetaGameState.Rules.Phases.Returns(_phases);
        _sut = new GameLoop(_gameRoot);
    }

    [Test]
    public void Advance_WhenNoAllowedActions_ReturnsGameAPIWithAllowedActions()
    {
        _phases[0].AllowedActions.Returns(new List<ActionDescription>(){ new (ActionType.PassTurn) });
        var result = _sut.Advance();
        
        result.Should().BeOfType<GameAPI>();
    }
    
    [Test]
    public void Advance_WhenActionQueueReturnsPromptEvent_ReturnsPromptApi()
    {
        
        _phases[0].AllowedActions.Returns(new List<ActionDescription>());
        _actionQueue.ResolveNextKeyword().Returns(new PromptEvent(default!, default!, default!, default!));
        var result = _sut.Advance();
    }
    
    [Test]
    public void Advance_WhenThereAreMultipleSteps_TheyAreResolvedInOrder()
    {
        var keyword1 = Substitute.For<IKeywordInstance>();
        var keyword2 = Substitute.For<IKeywordInstance>();
        var keyword3 = Substitute.For<IKeywordInstance>();
        
        var step1 = Substitute.For<IStep>();
        step1.Effects.Returns(new List<IKeywordInstance>(){ keyword1, keyword2 });
        var step2 = Substitute.For<IStep>();
        step2.Effects.Returns(new List<IKeywordInstance>(){ keyword3 });
        
        _phases[0].Steps.Returns(new List<IStep>(){ step1, step2 });
        _phases[0].AllowedActions.Returns(new List<ActionDescription>());
        
        _phases[1].AllowedActions.Returns(new List<ActionDescription>() { new (ActionType.PassTurn) });
        
        var result = _sut.Advance();
        
        _actionQueue.Received().Push(Arg.Is<IResolutionFrame>(x => x.Effects == step1.Effects));
        _actionQueue.Received().Push(Arg.Is<IResolutionFrame>(x => x.Effects == step2.Effects));
        
        result.AvailableActions.Should().ContainSingle(x => x.Type == ActionType.PassTurn);
    }

    [Test]
    public void EndPhase_UpdatesCurrentPhaseAndCallsAdvance()
    {
        var nextPhase = _phases[1];
        
        _sut.CurrentPhase.Returns(_phases[0]);
        _sut.Advance().Returns(new PromptApi());

        var result = _sut.EndPhase();
        
        _sut.CurrentPhase.Should().Be(nextPhase);
        result.Should().BeOfType<PromptApi>();
    }

    // Add more test methods to cover additional scenarios and edge cases as needed.
}