using Archetype.Framework.Effects.Atomic;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Archetype.Tests;

public class ChangeZoneTests
{
    [Fact]
    public void ChangeZoneEffect()
    {
        var currentZone = Substitute.For<IZone>();
        currentZone.Id.Returns(Guid.NewGuid());
        var targetZone = Substitute.For<IZone>();
        targetZone.Id.Returns(Guid.NewGuid());
        var atom = Substitute.For<IAtom>();
        atom.Zone.Returns(currentZone);
        atom.Id.Returns(Guid.NewGuid());
        
        var result = AtomicEffect.ChangeZone(atom, targetZone);
        
        result.Keyword.Should().Be("ChangeZone");
        result.Results.Should().BeEquivalentTo(
            ResultAssertions.Atomic("ChangeZone", new AtomicEffect.ChangeZoneResult(atom.Id, currentZone.Id, targetZone.Id)));
        
        targetZone.Received(1).AddAtom(atom);
        currentZone.Received(1).RemoveAtom(atom);
        atom.Zone.Should().Be(targetZone);
    }
    
    [Fact]
    public void ChangeZoneEffect_SameZone_ReturnsNoOp()
    {
        var zone = Substitute.For<IZone>();
        var atom = Substitute.For<IAtom>();
        atom.Zone.Returns(zone);
        
        var result = AtomicEffect.ChangeZone(atom, zone);
        
        result.Keyword.Should().Be("ChangeZone");
        result.Results.Should().BeEquivalentTo(
            ResultAssertions.NoOp("ChangeZone"));
        
        zone.DidNotReceive().AddAtom(atom);
        zone.DidNotReceive().RemoveAtom(atom);
        atom.Zone.Should().Be(zone);
    }

    [Fact]
    public void ChangeZoneEffect_NoCurrentZone()
    {
        var targetZone = Substitute.For<IZone>();
        targetZone.Id.Returns(Guid.NewGuid());
        var atom = Substitute.For<IAtom>();
        atom.Zone.Returns(default(IZone?));
        atom.Id.Returns(Guid.NewGuid());
        
        var result = AtomicEffect.ChangeZone(atom, targetZone);
        
        result.Keyword.Should().Be("ChangeZone");
        result.Results.Should().BeEquivalentTo(
            ResultAssertions.Atomic("ChangeZone", new AtomicEffect.ChangeZoneResult(atom.Id, null, targetZone.Id)));
        
        targetZone.Received(1).AddAtom(atom);
        atom.Zone.Should().Be(targetZone);
    }
}