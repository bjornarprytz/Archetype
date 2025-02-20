using Archetype.Framework.Core;
using Archetype.Framework.Effects.Atomic;
using Archetype.Framework.GameLoop;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Archetype.Tests;

public class CreateCardTests
{
    private readonly IResolutionContext _resolutionContext = Substitute.For<IResolutionContext>();
    private readonly IGameState _state = Substitute.For<IGameState>();
    private readonly ICardPool _cardPool = Substitute.For<ICardPool>();

    public CreateCardTests()
    {
        _resolutionContext.GetState().Returns(_state);
        _state.GetCardPool().Returns(_cardPool);
    }
    
    
    [Fact]
    public void CreateCardEffect()
    {
        const string cardName = "Lightning Bolt";
        
        var cardProto = Substitute.For<ICardProto>();
        cardProto.Name.Returns(cardName);
        _cardPool.GetCard(cardName).Returns(cardProto);
        
        var zone = Substitute.For<IZone>();
        zone.Id.Returns(Guid.NewGuid());
        zone.AddAtom(default!).ReturnsForAnyArgs(true);
        
        var result = AtomicEffect.CreateCard(_resolutionContext, cardName, zone);
        
        result.Keyword.Should().Be("CreateCard");
        result.Results.Should().ContainKey("CreateCard").WhoseValue.Should().Contain(x => AssertResult(x, cardName, zone));
                    
        zone.Received(1).AddAtom(Arg.Is<ICard>(x => x.GetName() == cardName && x.Zone == zone && x.GetProto() == cardProto));
        
    }
    private static bool AssertResult(object? x, string cardName, IZone zone)
    {
        return x is AtomicEffect.CreateCardResult c && c.Zone == zone.Id && c.Card != Guid.Empty && c.Name == cardName;
    }
    
    [Fact]
    public void CreateCardEffect_AddZoneFails_ReturnsNoOp()
    {
        var zone = Substitute.For<IZone>();
        zone.AddAtom(default!).ReturnsForAnyArgs(false);
        
        var result = AtomicEffect.CreateCard(_resolutionContext, "Some card", zone);
        
        result.Keyword.Should().Be("CreateCard");
        result.Results.Should().BeEquivalentTo(ResultAssertions.NoOp("CreateCard"));
    }
    
    [Fact]
    public void CreateCardEffect_CardIsNotInCardPool_ReturnsNoOp()
    {
        _cardPool.GetCard(default!).ReturnsForAnyArgs(default(ICardProto));
        
        var result = AtomicEffect.CreateCard(_resolutionContext, "Some card", Substitute.For<IZone>());
        
        result.Keyword.Should().Be("CreateCard");
        result.Results.Should().BeEquivalentTo(ResultAssertions.NoOp("CreateCard"));
    }
}