using Archetype.Framework.Core;
using Archetype.Framework.Effects.Atomic;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Archetype.Tests;

public class CreateCardTests
{
    [Fact]
    public void CreateCardEffect()
    {
        var card = Substitute.For<ICardProto>();
        card.Name.Returns("Lightning Bolt");
        var zone = Substitute.For<IZone>();
        zone.Id.Returns(Guid.NewGuid());
        zone.AddAtom(default!).ReturnsForAnyArgs(true);
        
        var result = AtomicEffect.CreateCard(card, zone);
        
        result.Keyword.Should().Be("CreateCard");
        result.Results.Should().ContainKey("CreateCard").WhoseValue.Should().Contain(x => AssertResult(x, card, zone));
                    
        zone.Received(1).AddAtom(Arg.Is<ICard>(x => x.GetName() == "Lightning Bolt" && x.Zone == zone));
        
    }
    private static bool AssertResult(object? x, ICardProto card, IZone zone)
    {
        return x is AtomicEffect.CreateCardResult { Name: "Lightning Bolt" } c && c.Zone == zone.Id && c.Card != Guid.Empty;
    }
    
    [Fact]
    public void CreateCardEffect_AddZoneFails_ReturnsNoOp()
    {
        var card = Substitute.For<ICardProto>();
        var zone = Substitute.For<IZone>();
        zone.AddAtom(default!).ReturnsForAnyArgs(false);
        
        var result = AtomicEffect.CreateCard(card, zone);
        
        result.Keyword.Should().Be("CreateCard");
        result.Results.Should().BeEquivalentTo(ResultAssertions.NoOp("CreateCard"));
    }
}