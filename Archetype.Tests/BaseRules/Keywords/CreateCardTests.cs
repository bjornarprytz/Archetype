using Archetype.Framework.BaseRules.Keywords.Primitive;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Design;
using Archetype.Framework.Extensions;
using Archetype.Framework.State;
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
    public void ShouldCreateCard()
    {
        // Arrange
        var protoCard = Substitute.For<IProtoCard>();
        protoCard.Name.Returns("TestCard");
        _context.MetaGameState.ProtoData.GetProtoCard("TestCard").Returns(protoCard);
        
        var payload = _sut.CreateInstance("TestCard", _targetZone).BindPayload(_context);

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