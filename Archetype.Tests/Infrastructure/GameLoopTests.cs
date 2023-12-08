using Archetype.Framework.BaseRules.Keywords.Primitive;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Core.Structure;
using Archetype.Framework.Interface;
using Archetype.Framework.Interface.Actions;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute.Extensions;

namespace Archetype.Tests.Infrastructure;

using NUnit.Framework;
using NSubstitute;
using System;
using System.Collections.Generic;

[TestFixture]
public class GameLoopTests
{
    private GameLoop _sut = default!;
    
    private IGameRoot _gameRoot;
    private IActionQueue _actionQueue;
    private IPhase _firstPhase;
    private IPhase _secondPhase;
    private IPhase _thirdPhase;

    [SetUp]
    public void SetUp()
    {
        _gameRoot = Substitute.For<IGameRoot>();
        _actionQueue = Substitute.For<IActionQueue>();
        _firstPhase = Substitute.For<IPhase>();
        _secondPhase = Substitute.For<IPhase>();
        _thirdPhase = Substitute.For<IPhase>();
        _actionQueue.ResolveNextKeyword().Returns(null as IEvent);
        
        _gameRoot.Infrastructure.ActionQueue.Returns(_actionQueue);
        _gameRoot.MetaGameState.ProtoData.TurnSequence.Returns(new List<IPhase>(){ _firstPhase, _secondPhase, _thirdPhase });
        
        _thirdPhase.AllowedActions.Returns(new List<ActionDescription>(){ new (ActionType.PassTurn) });
        
        _sut = new GameLoop(_gameRoot.Infrastructure.ActionQueue, _gameRoot.GameState, _gameRoot.MetaGameState);
    }
    
    [Test]
    public void Advance_AdvancesToTheNextPhaseWithAllowedActions_DoesNotAdvanceToNextPhase()
    {
        _sut.Advance();
        
        _sut.CurrentPhase.Should().Be(_thirdPhase);
        
        _sut.Advance();
        
        _sut.CurrentPhase.Should().Be(_thirdPhase);
    }
    
    [Test]
    public void Advance_WhenActionQueueReturnsPromptEvent_ReturnsPromptApi()
    {
        _firstPhase.AllowedActions.Returns(new List<ActionDescription>());
        _actionQueue.ResolveNextKeyword().Returns(new PromptEvent(default!, default!, default!, default!, default!, default!));
        var result = _sut.Advance();
    }
    
    [Test]
    public void Advance_WhenThereAreMultipleSteps_TheyAreResolvedInOrder()
    {
        var keyword1 = Substitute.For<IKeywordInstance>();
        var keyword2 = Substitute.For<IKeywordInstance>();
        var keyword3 = Substitute.For<IKeywordInstance>();
        
        _firstPhase.Steps.Returns(new List<IKeywordInstance>(){ keyword1, keyword2, keyword3 });
        _firstPhase.AllowedActions.Returns(new List<ActionDescription>());
        
        _secondPhase.AllowedActions.Returns(new List<ActionDescription>() { new (ActionType.PassTurn) });
        
        var result = _sut.Advance();
        
        _actionQueue.Received().Push(Arg.Is<IResolutionFrame>(x => x.Effects == _firstPhase.Steps));
        
        result.AvailableActions.Should().ContainSingle(x => x.Type == ActionType.PassTurn);
    }

    
    
    [Test]
    public void EndPhase_AdvancesToTheNextPhase_AndResetsTurnOrder()
    {
        _firstPhase.AllowedActions.Returns(new List<ActionDescription>() { new (ActionType.PassTurn) });

        _sut.EndPhase();
        
        _sut.CurrentPhase.Should().Be(_firstPhase);
        
        _sut.EndPhase();
        
        _sut.CurrentPhase.Should().Be(_thirdPhase);
        
        _sut.EndPhase();
        
        _sut.CurrentPhase.Should().Be(_firstPhase);
    }
    
    
}