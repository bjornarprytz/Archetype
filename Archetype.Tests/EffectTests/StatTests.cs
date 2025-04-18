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
    public void ChangeStatEffect(string statKey, int? current, int change, int expected)
    {
        var atom = Create.AtomWithStats(statKey, current);
        
        var result = AtomicEffect.ChangeStat(atom, statKey, change);
        
        result.Should().BeEquivalentTo(
        ResultAssertions.Atomic(
            "ChangeStat", atom, statKey, change));

        atom.State.Stats[statKey].Should().Be(expected);
    }
    
    [Fact]
    public void ChangeStatEffect_NoChange_ReturnsNoOp()
    {
        var atom = Create.AtomWithStats("someStat", 0);
        
        var result = AtomicEffect.ChangeStat(atom, "someStat", 0);
        
        result.Should().BeEquivalentTo(ResultAssertions.NoOp("ChangeStat", atom, "someStat", 0));

        atom.State.Stats["someStat"].Should().Be(0);
    }
    
    [Theory]
    [InlineData("health", 10, -1)]
    [InlineData("mana", 0, 1)]
    [InlineData("mana", null, 2)]
    public void SetStatEffect(string statName, int? current, int value)
    {
        var atom = Create.AtomWithStats(statName, current);
        
        var result = AtomicEffect.SetStat(atom, statName, value);
        
        result.Should().BeEquivalentTo(
            ResultAssertions.Atomic(
                "SetStat", atom, statName, value
                )
            );

        atom.State.Stats[statName].Should().Be(value);
    }
    
    [Fact]
    public void SetStatEffect_NoChange_ReturnsNoOp()
    {
        const int current = 1;
        const int value = 1;
        
        var atom = Create.AtomWithStats("someStat", current);
        
        var result = AtomicEffect.SetStat(atom, "someStat", value);
        
        result.Should().BeEquivalentTo(ResultAssertions.NoOp("SetStat", atom, "someStat", value));

        atom.State.Stats["someStat"].Should().Be(value);
    }
    
    [Fact]
    public void SetStatEffect_StatNotFound()
    {
        var atom = Create.BasicAtom();
        
        var result = AtomicEffect.SetStat(atom, "someStat", 0);
        
        result.Should().BeEquivalentTo(
            ResultAssertions.Atomic(
                "SetStat", atom, "someStat", 0
                ));

        atom.State.Stats["someStat"].Should().Be(0);
    }
}