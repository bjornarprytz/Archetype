using Archetype.Framework.BaseRules.Keywords;
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
    
    
    private IZone _targetZone = default!;
    private IResolutionContext _context = default!;
    
    [SetUp]
    public void Setup()
    {
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
        

        // Act
        var result = Effects.CreateCard(_context, "TestCard", _targetZone);
        
        // Assert
        _targetZone.Received(1).Add(Arg.Is<ICard>(c => c.Name == "TestCard"));
        _context.GameState.Received(1).AddAtom(Arg.Is<ICard>(c => c.Name == "TestCard"));
        result.Should().BeOfType<EffectResult>();
    }
}