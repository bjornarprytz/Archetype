using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Cards;
using Archetype.Core.Atoms.Zones;
using Archetype.Core.Effects;
using Archetype.Core.Infrastructure;
using Archetype.Rules.Actions;
using NSubstitute;

namespace Archetype.Rules.Tests;

public class ActionTests
{
    private IAtomFinder _atomFinder;
    private IGameState _gameState;

    
    private readonly Guid _cardToPlayId = Guid.NewGuid();
    private ICard _cardToPlay;
    private readonly Guid _cardToPayWithId = Guid.NewGuid();
    private ICard _cardToPayWith;
    private IHand _hand;
    private IResolution _resolutionZone;
    
    [SetUp]
    public void Setup()
    {
        _atomFinder = Substitute.For<IAtomFinder>();
        _gameState = Substitute.For<IGameState>();

        _cardToPlay = Substitute.For<ICard>();
        _cardToPayWith = Substitute.For<ICard>();
        
        _resolutionZone = Substitute.For<IResolution>();
        _hand = Substitute.For<IHand>();
        _hand.Contents.Returns(new List<ICard>
        {
            _cardToPlay,
            _cardToPayWith
        });

        _gameState.Player.Hand.Returns(_hand);
        _gameState.ResolutionZone.Returns(_resolutionZone);
        
        _cardToPlay.CurrentZone.Returns(_hand);
        _cardToPayWith.CurrentZone.Returns(_hand);

        _atomFinder.FindAtom<ICard>(_cardToPlayId).Returns(_cardToPlay);
        _atomFinder.FindAtom<ICard>(_cardToPayWithId).Returns(_cardToPayWith);
    }
    
    [Test]
    public void PlayCardHandler_NoTargets_CardToPlayResolvesWithRightContext()
    {
        var playCardHandler = new PlayCard.Handler(_gameState, _atomFinder);

        var result = playCardHandler.Handle(new PlayCard.Command(_cardToPlayId, new List<Guid> { _cardToPayWithId }, new List<Guid>()), CancellationToken.None);

        _cardToPlay.Proto.Received(1).Resolve(Arg.Is<IContext<ICard>>(c => 
                c.Source == _cardToPlay && c.GameState == _gameState));
    }
    
    [Test]
    public void PlayCardHandler_TwoTargets_CardToPlayResolvesWithRightContext()
    {
        // TODO: Is this setup a sign that I need to isolate the binding of targets to the card?
        var guid1 = Guid.NewGuid();
        var target1 = Substitute.For<IAtom>();
        var descriptor1 = Substitute.For<ITargetDescriptor>();
        descriptor1.TargetIndex.Returns(0);
        descriptor1.TargetType.Returns(typeof(IAtom));
        _atomFinder.FindAtom(guid1).Returns(target1);
        var guid2 = Guid.NewGuid();
        var target2 = Substitute.For<IAtom>();
        var descriptor2 = Substitute.For<ITargetDescriptor>();
        descriptor2.TargetIndex.Returns(1);
        descriptor2.TargetType.Returns(typeof(IAtom));
        _atomFinder.FindAtom(guid2).Returns(target2);
        
        _cardToPlay.Proto.TargetDescriptors.Returns(new List<ITargetDescriptor>
        {
            descriptor1,
            descriptor2
        });
        
        var playCardHandler = new PlayCard.Handler(_gameState, _atomFinder);

        var result = playCardHandler.Handle(new PlayCard.Command(_cardToPlayId, new List<Guid> { _cardToPayWithId }, new List<Guid>{ guid1, guid2 }), CancellationToken.None);

        _cardToPlay.Proto.Received(1).Resolve(Arg.Is<IContext<ICard>>(c => 
                c.Source == _cardToPlay && c.GameState == _gameState 
            && c.TargetProvider.GetTarget<IAtom>(0) == target1 && c.TargetProvider.GetTarget<IAtom>(1) == target2));
    }
}