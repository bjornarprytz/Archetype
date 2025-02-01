using Archetype.Framework.Effects.Atomic;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Archetype.Tests;

public class FacetsTests
{
    [Theory]
    [InlineData()]
    [InlineData("a", "b", "c")]
    [InlineData("b", "c", "d")]
    public void SetFacetEffect(params string[] existingFacets)
    {
        var facetsToSet = new[] { "a", "b", "c" };
        
        var atom = Substitute.For<IAtom>();
        
        atom.GetFacet("someFacet").Returns(existingFacets);
        
        var result = AtomicEffect.SetFacet(atom, "someFacet", facetsToSet);
        
        var expectedRemovals = existingFacets.Except(facetsToSet).ToArray();
        var expectedAdditions = facetsToSet.Except(existingFacets).ToArray();
        
        result.Keyword.Should().Be("SetFacet");
        
        if (expectedRemovals.Length == 0 && expectedAdditions.Length == 0)
        {
            result.Results.Should().BeEquivalentTo(ResultAssertions.NoOp("SetFacet"));
            atom.DidNotReceiveWithAnyArgs().SetFacet(default!, default!);
        }
        else
        {
            result.Results.Should().BeEquivalentTo(ResultAssertions.Atomic("SetFacet", new AtomicEffect.SetFacetResult("someFacet", expectedAdditions, expectedRemovals)));
            atom.Received(1).SetFacet("someFacet", Arg.Is<string[]>(a => a.SequenceEqual(facetsToSet)));
        }
    }
    
    [Fact]
    public void SetFacetEffect_RemovesDuplicates()
    {
        var facetsToSet = new[] { "a", "b", "b" };
        
        var atom = Substitute.For<IAtom>();
        
        var result = AtomicEffect.SetFacet(atom, "someFacet", facetsToSet);
        
        result.Keyword.Should().Be("SetFacet");
        result.Results.Should().BeEquivalentTo(ResultAssertions.Atomic("SetFacet", new AtomicEffect.SetFacetResult("someFacet", facetsToSet.Distinct().ToArray(), Array.Empty<string>())));
        
        atom.Received(1).SetFacet("someFacet",  Arg.Is<string[]>(a => a.SequenceEqual(facetsToSet.Distinct())));
    }
    
    [Fact]
    public void SetFacetEffect_EmptyFacet_ReturnsNoOp()
    {
        var atom = Substitute.For<IAtom>();
        
        var result = AtomicEffect.SetFacet(atom, "someFacet", Array.Empty<string>());
        
        result.Keyword.Should().Be("SetFacet");
        result.Results.Should().BeEquivalentTo(ResultAssertions.NoOp("SetFacet"));
        
        atom.DidNotReceiveWithAnyArgs().SetFacet(default!, default!);
    }
    
    [Fact]
    public void RemoveFacetsEffect()
    {
        var existingFacets = new[] { "a", "b" };
        var facetsToRemove = new[] { "b", "c" };
        var expectedRemovals = new[] { "b" };
        var expectedRemaining = new[] { "a" };
        
        var atom = Substitute.For<IAtom>();
        
        atom.GetFacet("someFacet").Returns(existingFacets);
        
        var result = AtomicEffect.RemoveFacets(atom, "someFacet", facetsToRemove);
        
        result.Keyword.Should().Be("RemoveFacets");
        result.Results.Should().BeEquivalentTo(ResultAssertions.Atomic("RemoveFacets", new AtomicEffect.RemoveFacetsResult("someFacet", expectedRemovals)));
        
        atom.Received(1).SetFacet("someFacet", Arg.Is<string[]>(a => a.SequenceEqual(expectedRemaining)));
    }
    
    [Fact]
    public void RemoveFacetsEffect_NoFacet_ReturnsNoOp()
    {
        var atom = Substitute.For<IAtom>();
        
        var result = AtomicEffect.RemoveFacets(atom, "someFacet", new[] { "a" });
        
        result.Keyword.Should().Be("RemoveFacets");
        result.Results.Should().BeEquivalentTo(ResultAssertions.NoOp("RemoveFacets"));
        
        atom.DidNotReceiveWithAnyArgs().SetFacet(default!, default!);
    }
    
    [Fact]
    public void RemoveFacetsEffect_NoRemovals_ReturnsNoOp()
    {
        var existingFacets = new[] { "a", "b" };
        
        var atom = Substitute.For<IAtom>();
        
        atom.GetFacet("someFacet").Returns(existingFacets);
        
        var result = AtomicEffect.RemoveFacets(atom, "someFacet", new[] { "c" });
        
        result.Keyword.Should().Be("RemoveFacets");
        result.Results.Should().BeEquivalentTo(ResultAssertions.NoOp("RemoveFacets"));
        
        atom.DidNotReceiveWithAnyArgs().SetFacet(default!, default!);
    }
    
    [Fact]
    public void ClearFacetEffect()
    {
        var existingFacets = new[] { "a", "b" };
        
        var atom = Substitute.For<IAtom>();
        
        atom.GetFacet("someFacet").Returns(existingFacets);
        
        var result = AtomicEffect.ClearFacet(atom, "someFacet");
        
        result.Keyword.Should().Be("ClearFacet");
        result.Results.Should().BeEquivalentTo(ResultAssertions.Atomic("ClearFacet", new AtomicEffect.RemoveFacetsResult("someFacet", existingFacets)));
        
        atom.Received(1).RemoveFacet("someFacet");
    }
    
    [Fact]
    public void ClearFacetEffect_NoFacet_ReturnsNoOp()
    {
        var atom = Substitute.For<IAtom>();
        
        var result = AtomicEffect.ClearFacet(atom, "someFacet");
        
        result.Keyword.Should().Be("ClearFacet");
        result.Results.Should().BeEquivalentTo(ResultAssertions.NoOp("ClearFacet"));
        
        atom.DidNotReceiveWithAnyArgs().RemoveFacet(default!);
    }
    
    [Fact]
    public void ClearFacetEffect_EmptyFacet_ReturnsNoOp()
    {
        var atom = Substitute.For<IAtom>();
        
        atom.GetFacet("someFacet").Returns(Array.Empty<string>());
        
        var result = AtomicEffect.ClearFacet(atom, "someFacet");
        
        result.Keyword.Should().Be("ClearFacet");
        result.Results.Should().BeEquivalentTo(ResultAssertions.NoOp("ClearFacet"));
        
        atom.DidNotReceiveWithAnyArgs().RemoveFacet(default!);
    }
    
    [Fact]
    public void AddFacetsEffect()
    {
        var existingFacets = new[] { "a", "b" };
        var facetsToAdd = new[] { "b", "c" };
        var expectedAdditions = new[] { "c" };
        var expectedRemaining = new[] { "a", "b", "c" };
        
        var atom = Substitute.For<IAtom>();
        
        atom.GetFacet("someFacet").Returns(existingFacets);
        
        var result = AtomicEffect.AddFacets(atom, "someFacet", facetsToAdd);
        
        result.Keyword.Should().Be("AddFacets");
        result.Results.Should().BeEquivalentTo(ResultAssertions.Atomic("AddFacets", new AtomicEffect.AddFacetsResult("someFacet", expectedAdditions)));
        
        atom.Received(1).SetFacet("someFacet", Arg.Is<string[]>(a => a.SequenceEqual(expectedRemaining)));
    }
    
    [Fact]
    public void AddFacetsEffect_NoAdditions_ReturnsNoOp()
    {
        var existingFacets = new[] { "a", "b" };
        
        var atom = Substitute.For<IAtom>();
        
        atom.GetFacet("someFacet").Returns(existingFacets);
        
        var result = AtomicEffect.AddFacets(atom, "someFacet", new[] { "b" });
        
        result.Keyword.Should().Be("AddFacets");
        result.Results.Should().BeEquivalentTo(ResultAssertions.NoOp("AddFacets"));
        
        atom.DidNotReceiveWithAnyArgs().SetFacet(default!, default!);
    }
}