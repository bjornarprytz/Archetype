using Archetype.Framework.Core;
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

        var atom = Create.AtomWithFacets("someFacet", existingFacets);
        
        var result = AtomicEffect.SetFacet(atom, "someFacet", facetsToSet);
        
        var expectedRemovals = existingFacets.Except(facetsToSet).ToArray();
        var expectedAdditions = facetsToSet.Except(existingFacets).ToArray();
        
        if (expectedRemovals.Length == 0 && expectedAdditions.Length == 0)
        {
            result.Should().BeEquivalentTo(ResultAssertions.NoOp("SetFacet"));
        }
        else
        {
            result.Should().BeEquivalentTo(ResultAssertions.Atomic("SetFacet", new AtomicEffect.SetFacetResult("someFacet", expectedAdditions, expectedRemovals)));
        }
        
        atom.BaseState.Facets["someFacet"].Should().BeEquivalentTo(facetsToSet);
    }
    
    [Fact]
    public void SetFacetEffect_RemovesDuplicates()
    {
        var facetsToSet = new[] { "a", "b", "b" };

        var atom = Create.BasicAtom();
        
        var result = AtomicEffect.SetFacet(atom, "someFacet", facetsToSet);
        
        result.Should().BeEquivalentTo(ResultAssertions.Atomic("SetFacet", new AtomicEffect.SetFacetResult("someFacet", facetsToSet.Distinct().ToArray(), Array.Empty<string>())));
        
        atom.BaseState.Facets["someFacet"].Should().BeEquivalentTo(facetsToSet.Distinct());
    }
    
    [Fact]
    public void SetFacetEffect_EmptyFacet_ReturnsNoOp()
    {
        var atom = Create.BasicAtom();
        
        var result = AtomicEffect.SetFacet(atom, "someFacet", Array.Empty<string>());
        
        result.Should().BeEquivalentTo(ResultAssertions.NoOp("SetFacet"));
        
        atom.BaseState.Facets.Should().NotContainKey("someFacet");
    }
    
    [Fact]
    public void RemoveFacetsEffect()
    {
        var existingFacets = new[] { "a", "b" };
        var facetsToRemove = new[] { "b", "c" };
        var expectedRemovals = new[] { "b" };
        var expectedRemaining = new[] { "a" };

        var atom = Create.AtomWithFacets("someFacet", existingFacets);
        
        var result = AtomicEffect.RemoveFacets(atom, "someFacet", facetsToRemove);
        
        result.Should().BeEquivalentTo(ResultAssertions.Atomic("RemoveFacets", new AtomicEffect.RemoveFacetsResult("someFacet", expectedRemovals)));
        
        atom.BaseState.Facets["someFacet"].Should().BeEquivalentTo(expectedRemaining);
    }
    
    [Fact]
    public void RemoveFacetsEffect_NoFacet_ReturnsFailure()
    {
        var atom = Create.BasicAtom();
        
        var result = AtomicEffect.RemoveFacets(atom, "someFacet", new[] { "a" });
        
        result.Should().BeEquivalentTo(ResultAssertions.Failure("RemoveFacets"));
        
        atom.BaseState.Facets.Should().NotContainKey("someFacet");
    }
    
    [Fact]
    public void RemoveFacetsEffect_NoRemovals_ReturnsFailure()
    {
        var existingFacets = new[] { "a", "b" };
        
        var atom = Create.AtomWithFacets("someFacet", existingFacets);
        
        var result = AtomicEffect.RemoveFacets(atom, "someFacet", new[] { "c" });
        
        result.Should().BeEquivalentTo(ResultAssertions.Failure("RemoveFacets"));
        
        atom.BaseState.Facets["someFacet"].Should().BeEquivalentTo(existingFacets);
    }
    
    [Fact]
    public void ClearFacetEffect()
    {
        var existingFacets = new[] { "a", "b" };
        
        var atom = Create.AtomWithFacets("someFacet", existingFacets);
        
        var result = AtomicEffect.ClearFacet(atom, "someFacet");
        
        result.Should().BeEquivalentTo(ResultAssertions.Atomic("ClearFacet", new AtomicEffect.RemoveFacetsResult("someFacet", existingFacets)));
        
        atom.BaseState.Facets.Should().NotContainKey("someFacet");
    }
    
    [Fact]
    public void ClearFacetEffect_NoFacet_ReturnsFailure()
    {
        var atom = Create.BasicAtom();
        
        var result = AtomicEffect.ClearFacet(atom, "someFacet");
        
        result.Should().BeEquivalentTo(ResultAssertions.Failure("ClearFacet"));
        
        atom.BaseState.Facets.Should().NotContainKey("someFacet");
    }
    
    [Fact]
    public void AddFacetsEffect()
    {
        var existingFacets = new[] { "a", "b" };
        var facetsToAdd = new[] { "b", "c" };
        var expectedAdditions = new[] { "c" };
        var expectedRemaining = new[] { "a", "b", "c" };
        
        var atom = Create.AtomWithFacets("someFacet", existingFacets);
        
        var result = AtomicEffect.AddFacets(atom, "someFacet", facetsToAdd);
        
        result.Should().BeEquivalentTo(ResultAssertions.Atomic("AddFacets", new AtomicEffect.AddFacetsResult("someFacet", expectedAdditions)));
        
        atom.BaseState.Facets["someFacet"].Should().BeEquivalentTo(expectedRemaining);
    }
    
    [Fact]
    public void AddFacetsEffect_NoAdditions_ReturnsNoOp()
    {
        var existingFacets = new[] { "a", "b" };
        
        var atom = Create.AtomWithFacets("someFacet", existingFacets);
        
        var result = AtomicEffect.AddFacets(atom, "someFacet", new[] { "b" });
        
        result.Should().BeEquivalentTo(ResultAssertions.NoOp("AddFacets"));
        
        atom.BaseState.Facets["someFacet"].Should().BeEquivalentTo(existingFacets);
    }
}