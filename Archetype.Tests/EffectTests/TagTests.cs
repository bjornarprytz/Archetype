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
        var atom = Create.BasicAtom();
        
        var result = AtomicEffect.AddTag(atom, "someTag");
        
        result.Should().BeEquivalentTo(ResultAssertions.Atomic("AddTag", atom, "someTag"));
        
        atom.State.Tags.Should().Contain("someTag");
    }
    
    [Fact]
    public void RemoveTagEffect()
    {
        var atom = Create.AtomWithTags("someTag");
        
        var result = AtomicEffect.RemoveTag(atom, "someTag");
        
        result.Should().BeEquivalentTo(ResultAssertions.Atomic("RemoveTag", atom, "someTag"));
        
        atom.State.Tags.Should().NotContain("someTag");
    }
    
    
    
    [Fact]
    public void AddTagEffect_HasTag_ReturnsNoOp()
    {
        var atom = Create.AtomWithTags("someTag");
        
        var result = AtomicEffect.AddTag(atom, "someTag");
        
        result.Should().BeEquivalentTo(
            ResultAssertions.NoOp("AddTag", atom, "someTag"));
        
        atom.State.Tags.Should().Contain("someTag");
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AddTagEffect_TagIsNullOrEmpty_ReturnsNoOp(string? tag)
    {
        var atom = Create.BasicAtom();
        
        var result = AtomicEffect.AddTag(atom, tag!);
        
        result.Should().BeEquivalentTo(ResultAssertions.NoOp("AddTag", atom, tag));
        
        atom.State.Tags.Should().NotContain(tag);
    }
    
    
    [Fact]
    public void RemoveTagEffect_DoesNotHaveTag_ReturnsNoOp()
    {
        var atom = Create.BasicAtom();
        
        var result = AtomicEffect.RemoveTag(atom, "someTag");
        
        result.Should().BeEquivalentTo(
            ResultAssertions.NoOp("RemoveTag", atom, "someTag"));
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void RemoveTagEffect_TagIsNullOrEmpty_ReturnsNoOp(string? tag)
    {
        var atom = Create.BasicAtom();
        
        var result = AtomicEffect.RemoveTag(atom, tag!);
        
        result.Should().BeEquivalentTo(ResultAssertions.NoOp("RemoveTag", atom, tag));
        
        atom.State.Tags.Should().NotContain(tag);
    }
}