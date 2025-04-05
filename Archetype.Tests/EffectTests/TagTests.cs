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
        
        result.Should().BeEquivalentTo(ResultAssertions.Atomic("AddTag", new AtomicEffect.AddTagResult("someTag")));
        
        atom.BaseState.Tags.Should().Contain("someTag");
    }
    
    [Fact]
    public void RemoveTagEffect()
    {
        var atom = Create.AtomWithTags("someTag");
        
        var result = AtomicEffect.RemoveTag(atom, "someTag");
        
        result.Should().BeEquivalentTo(ResultAssertions.Atomic("RemoveTag", new AtomicEffect.RemoveTagResult("someTag")));
        
        atom.BaseState.Tags.Should().NotContain("someTag");
    }
    
    
    
    [Fact]
    public void AddTagEffect_HasTag_ReturnsFailure()
    {
        var atom = Create.AtomWithTags("someTag");
        
        var result = AtomicEffect.AddTag(atom, "someTag");
        
        result.Should().BeEquivalentTo(
            ResultAssertions.Failure("AddTag"));
        
        atom.BaseState.Tags.Should().Contain("someTag");
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AddTagEffect_TagIsNullOrEmpty_ReturnsFailure(string? tag)
    {
        var atom = Create.BasicAtom();
        
        var result = AtomicEffect.AddTag(atom, tag!);
        
        result.Should().BeEquivalentTo(ResultAssertions.Failure("AddTag"));
        
        atom.BaseState.Tags.Should().NotContain(tag);
    }
    
    
    [Fact]
    public void RemoveTagEffect_DoesNotHaveTag_ReturnsFailure()
    {
        var atom = Create.BasicAtom();
        
        var result = AtomicEffect.RemoveTag(atom, "someTag");
        
        result.Should().BeEquivalentTo(
            ResultAssertions.Failure("RemoveTag"));
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void RemoveTagEffect_TagIsNullOrEmpty_ReturnsFailure(string? tag)
    {
        var atom = Create.BasicAtom();
        
        var result = AtomicEffect.RemoveTag(atom, tag!);
        
        result.Should().BeEquivalentTo(ResultAssertions.Failure("RemoveTag"));
        
        atom.BaseState.Tags.Should().NotContain(tag);
    }
}