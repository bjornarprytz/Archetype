using Archetype.Framework.Effects.Atomic;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Archetype.Tests;

public class StatTests
{
    [Theory]
    [InlineData("damage", 1, 1, 2)]
    [InlineData("health", 10, -1, 9)]
    [InlineData("mana", 0, 1, 1)]
    [InlineData("mana", null, 2, 2)]
    public void ChangeStatEffect(string statName, int? current, int change, int expected)
    {
        var atom = Substitute.For<IAtom>();
        atom.GetStat(statName).Returns(current);
        
        var result = AtomicEffect.ChangeStat(atom, statName, change);
        
        result.Keyword.Should().Be("ChangeStat");
        result.Results.Should().BeEquivalentTo(
            ResultAssertions.Atomic(
                "ChangeStat", new AtomicEffect.StatChangeResult(statName, change)));

        atom.Received(1).SetStat(statName, expected);
    }
    
    [Fact]
    public void ChangeStatEffect_NoChange_ReturnsNoOp()
    {
        var atom = Substitute.For<IAtom>();
        
        var result = AtomicEffect.ChangeStat(atom, "someStat", 0);
        
        result.Keyword.Should().Be("ChangeStat");
        result.Results.Should().BeEquivalentTo(ResultAssertions.NoOp("ChangeStat"));

        atom.DidNotReceiveWithAnyArgs().SetStat(default!, default!);
    }
    
    [Theory]
    [InlineData("health", 10, -1, -11)]
    [InlineData("mana", 0, 1, 1)]
    [InlineData("mana", null, 2, 2)]
    public void SetStatEffect(string statName, int? current, int value, int expectedChange)
    {
        var atom = Substitute.For<IAtom>();
        atom.GetStat(statName).Returns(current);
        
        var result = AtomicEffect.SetStat(atom, statName, value);
        
        result.Keyword.Should().Be("SetStat");
        result.Results.Should().BeEquivalentTo(
            ResultAssertions.Atomic(
                "SetStat", new AtomicEffect.StatChangeResult(statName, expectedChange
                    )
                )
            );

        atom.Received(1).SetStat(statName, value);
    }
    
    [Theory]
    [InlineData(0, 0)]
    [InlineData(null, 0)]
    public void SetStatEffect_NoChange_ReturnsNoOp(int? current, int value)
    {
        var atom = Substitute.For<IAtom>();
        atom.GetStat("someStat").Returns(current);
        
        var result = AtomicEffect.SetStat(atom, "someStat", value);
        
        result.Keyword.Should().Be("SetStat");
        result.Results.Should().BeEquivalentTo(ResultAssertions.NoOp("SetStat"));

        atom.DidNotReceiveWithAnyArgs().SetStat(default!, default!);
    }
}