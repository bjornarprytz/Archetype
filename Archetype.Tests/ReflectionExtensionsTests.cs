using Archetype.Framework.GameLoop;
using Archetype.Framework.Parsing;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace Archetype.Tests;

public class ReflectionExtensionsTests
{
    [Fact]
    public void CreateAccessor_CreatesAccessorForGroup()
    {
        var context = Substitute.For<IResolutionContext>();
        var state = Substitute.For<IGameState>();
        var hand = Substitute.For<IZone>();
        var atom = Substitute.For<IAtom>();
        IAtom[] atoms = { atom };
        
        context.GetState().ReturnsForAnyArgs(state);
        state.GetHand().ReturnsForAnyArgs(hand);
        hand.GetAtoms().ReturnsForAnyArgs(atoms);
        
        
        var path = "state.hand.atoms".Split('.');
        
        var accessor = path.CreateAccessor<IResolutionContext, IEnumerable<IAtom>>();
        
        
        var result = accessor(context);
        
        result.Should().BeSameAs(atoms);
    }
    
    [Fact]
    public void CreateAccessor_CreatesAccessorForProperty()
    {
        var context = Substitute.For<IResolutionContext>();
        var state = Substitute.For<IGameState>();
        var hand = Substitute.For<IZone>();
        
        context.GetState().ReturnsForAnyArgs(state);
        state.GetHand().ReturnsForAnyArgs(hand);
        hand.AtomCount.Returns(69);
        
        
        var path = "state.hand.count".Split('.');
        
        var accessor = path.CreateAccessor<IResolutionContext, int?>();
        
        
        var result = accessor(context);
        
        result.Should().Be(69);
    }
    
    [Fact]
    public void CreateAccessor_WhenPartOfThePathIsNull_ReturnsNull()
    {
        var context = Substitute.For<IResolutionContext>();
        context.GetState().ReturnsNull();
        
        var path = "state.hand.count".Split('.');
        
        var accessor = path.CreateAccessor<IResolutionContext, int?>();
        
        var result = accessor(context);
        
        result.Should().Be(null);
    }
    
    [Fact]
    public void CreateAccessor_CreatesAccessorForAtom()
    {
        var atom = Substitute.For<IAtom>();
        atom.GetStat("test").Returns(42);
        
        var path = "stats:test".Split('.');
        
        var accessor = path.CreateAccessor<IAtom, int?>();
        
        var result = accessor(atom);
        
        result.Should().Be(42);
    }
    
    [Fact]
    public void CreateAccessor_NonExistentPath_Throws()
    {
        var path = "non.existent.path".Split('.');
        
        var act = () => path.CreateAccessor<IResolutionContext, int>();
        
        act.Should().Throw<InvalidOperationException>();
    }
    
    [Fact]
    public void CreateAccessor_UnfulfilledParameter_Throws()
    {
        var path = "stats".Split('.');
        
        var act = () => path.CreateAccessor<IResolutionContext, int>();
        
        act.Should().Throw<InvalidOperationException>();
    }
    
    [Fact]
    public void CreateAccessor_WithNumberParameter_MapsCorrectly()
    {
        var context = Substitute.For<IResolutionContext>();
        var target1 = Substitute.For<IAtom>();
        context.GetTarget(1).Returns(target1);
        
        var path = "targets:1".Split('.');
        
        var accessor = path.CreateAccessor<IResolutionContext, IAtom>();
        
        var result = accessor(context);
        
        result.Should().BeSameAs(target1);
    }
    
    [Theory]
    [InlineData("targets:1")]
    [InlineData("tarGets:1")]
    [InlineData("Targets:1")]
    [InlineData("tArGeTs:1")]
    public void CreateAccessor_PathPartsAreCaseInsensitive(string path)
    {
        var context = Substitute.For<IResolutionContext>();
        var target1 = Substitute.For<IAtom>();
        context.GetTarget(1).Returns(target1);
        
        
        var accessor = path.Split('.').CreateAccessor<IResolutionContext, IAtom>();
        
        var result = accessor(context);
        
        result.Should().BeSameAs(target1);
    }
}