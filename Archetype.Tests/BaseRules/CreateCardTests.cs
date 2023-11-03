using Archetype.BasicRules.Primitives;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;
using FluentAssertions;
using NSubstitute;

namespace Archetype.Tests.BaseRules;

[TestFixture]
public class CreateCardTests
{
    
    private CreateCard _sut = default!;
    
    private IZone _targetZone = default!;
    private IResolutionContext _context = default!;
    
    [SetUp]
    public void Setup()
    {
        _sut = new CreateCard();
        _targetZone = Substitute.For<IZone>();
        _context = Substitute.For<IResolutionContext>();
    }
    
    [Test]
    public void ShouldHaveCorrectName()
    {
        _sut.Name.Should().Be("CREATE_CARD");
    }
    
    [Test]
    public void ShouldCreateCard()
    {
        // Arrange
        var protoCard = Substitute.For<IProtoCard>();
        protoCard.Name.Returns("TestCard");
        _context.MetaGameState.ProtoCards.GetProtoCard("TestCard").Returns(protoCard);
        
        var payload = new EffectPayload(
            Guid.NewGuid(),
            Substitute.For<IAtom>(),
            _sut.Name,
            new object[] { "TestCard" },
            new [] { _targetZone }
        );

        // Act
        var result = _sut.Resolve(_context, payload);

        // Assert
        _targetZone.Received().Add(Arg.Is<ICard>(c => c.Name == "TestCard"));
        _context.GameState.Atoms.Received().Add(Arg.Any<Guid>(), Arg.Is<ICard>(c => c.Name == "TestCard"));
        result.Should().BeOfType<CreateCardEvent>();
        result.As<CreateCardEvent>().Card.Name.Should().Be("TestCard");
        result.As<CreateCardEvent>().Zone.Should().Be(_targetZone);
        result.As<CreateCardEvent>().Source.Should().Be(payload.Source);
    }
}