using Archetype.Framework.Effects.Atomic;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Archetype.Tests;

public class TagTests
{
    [Fact]
    public void AddTagEffect()
    {
        var atom = Substitute.For<IAtom>();
        
        var result = AtomicEffect.AddTag(atom, "someTag");
        
        result.Keyword.Should().Be("AddTag");
        result.Results.Should().BeEquivalentTo(ResultAssertions.Atomic("AddTag", new AtomicEffect.AddTagResult("someTag")));
        
        atom.Received(1).AddTag("someTag");
    }
    
    [Fact]
    public void RemoveTagEffect()
    {
        var atom = Substitute.For<IAtom>();
        atom.HasTag("someTag").Returns(true);
        
        var result = AtomicEffect.RemoveTag(atom, "someTag");
        
        result.Keyword.Should().Be("RemoveTag");
        result.Results.Should().BeEquivalentTo(ResultAssertions.Atomic("RemoveTag", new AtomicEffect.RemoveTagResult("someTag")));
        
        atom.Received(1).RemoveTag("someTag");
    }
    
    
    
    [Fact]
    public void AddTagEffect_HasTag_ReturnsNoOp()
    {
        var atom = Substitute.For<IAtom>();
        atom.HasTag("someTag").Returns(true);
        
        var result = AtomicEffect.AddTag(atom, "someTag");
        
        result.Keyword.Should().Be("AddTag");
        result.Results.Should().BeEquivalentTo(
            ResultAssertions.NoOp("AddTag"));
        
        atom.DidNotReceive().AddTag("someTag");
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AddTagEffect_TagIsNullOrEmpty_ReturnsNoOp(string? tag)
    {
        var atom = Substitute.For<IAtom>();
        
        var result = AtomicEffect.AddTag(atom, tag!);
        
        result.Keyword.Should().Be("AddTag");
        result.Results.Should().BeEquivalentTo(ResultAssertions.NoOp("AddTag"));
        
        atom.DidNotReceiveWithAnyArgs().AddTag(default!);
    }
    
    
    [Fact]
    public void RemoveTagEffect_DoesNotHaveTag_ReturnsNoOp()
    {
        var atom = Substitute.For<IAtom>();
        atom.HasTag("someTag").Returns(false);
        
        var result = AtomicEffect.RemoveTag(atom, "someTag");
        
        result.Keyword.Should().Be("RemoveTag");
        result.Results.Should().BeEquivalentTo(
            ResultAssertions.NoOp("RemoveTag"));
        
        atom.DidNotReceive().RemoveTag("someTag");
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void RemoveTagEffect_TagIsNullOrEmpty_ReturnsNoOp(string? tag)
    {
        var atom = Substitute.For<IAtom>();
        
        var result = AtomicEffect.RemoveTag(atom, tag!);
        
        result.Keyword.Should().Be("RemoveTag");
        result.Results.Should().BeEquivalentTo(ResultAssertions.NoOp("RemoveTag"));
        
        atom.DidNotReceiveWithAnyArgs().RemoveTag(default!);
    }
}