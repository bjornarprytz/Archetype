using Archetype.Framework.Effects.Atomic;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Archetype.Tests;

public class ResourceChangeTests
{
    private readonly IPlayer _player = Substitute.For<IPlayer>();
    private readonly IGameState _state = Substitute.For<IGameState>();
    private readonly IResolutionContext _context = Substitute.For<IResolutionContext>();

    public ResourceChangeTests()
    {
        _context.GetState().Returns(_state);
        _state.GetPlayer().Returns(_player);
    }
    
    [Fact]
    public void PayResourceEffect_AmountIsEqualToCost_PaymentSuccessful()
    {
        const string resourceKey = "resource";
        const int currentAmount = 3;
        const int amountToPay = 3;
        
        _player.GetResouceCount(resourceKey).Returns(currentAmount);
        
        var result = AtomicEffect.PayResource(_context, resourceKey, amountToPay);
        
        result.Keyword.Should().Be("PayResource");
        result.Results.Should().BeEquivalentTo(ResultAssertions.Atomic("PayResource", new AtomicEffect.ResourceChangeResult(resourceKey, -amountToPay, currentAmount - amountToPay)));
        
        _player.Received(1).SetResourceCount(resourceKey, currentAmount - amountToPay);
    }
    
    [Theory]
    [InlineData(10, 11)]
    [InlineData(1, 0)]
    [InlineData(0, 1)]
    [InlineData(0, -1)]
    public void PayResourceEffect_InvalidAmount_ReturnsNoOp(int currentAmount, int amountToPay)
    {
        const string resourceKey = "resource";
        
        _player.GetResouceCount(resourceKey).Returns(currentAmount);
        
        var result = AtomicEffect.PayResource(_context, resourceKey, amountToPay);
        
        result.Keyword.Should().Be("PayResource");
        result.Results.Should().BeEquivalentTo(ResultAssertions.NoOp("PayResource"));
        
        _player.DidNotReceive().SetResourceCount(resourceKey, Arg.Any<int>());
    }
    
}