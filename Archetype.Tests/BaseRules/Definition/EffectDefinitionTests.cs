using System.Reflection;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.State;
using Archetype.Tests.MockUtilities;
using FluentAssertions;
using NSubstitute;

namespace Archetype.Tests.BaseRules.Definition;

[TestFixture]
public class EffectDefinitionTests
{
    private static IEffectResult NoOperandsFunc(IResolutionContext context) =>  EffectResult.Resolved;
    private static IEffectResult ThreeOperandsFunc(IResolutionContext context, IAtom atom, string str, int number) =>  EffectResult.Resolved;
    
    private IResolutionContext _context = default!;
    
    [SetUp]
    public void SetUp()
    {
        _context = Substitute.For<IResolutionContext>();
    }
    
    [Test]
    public void Resolve_WhenKeywordDoesNotMatch_ThrowsException()
    {
        var sut = new EffectDefinition(NoOperandsFunc);
        var keywordInstance = Substitute.For<IKeywordInstance>();
        keywordInstance.Keyword.Returns("NotTheSameKeyword");
        
        var act = () => sut.Resolve(_context, keywordInstance);
        
        act.Should().Throw<InvalidOperationException>();
    }
    
    [Test]
    public void Resolve_WhenEffectResolves_ReturnsResult()
    {
        var sut = new EffectDefinition(NoOperandsFunc);
        var keywordInstance = Setup.KeywordInstance("NoOperandsFunc");
        
        var result = sut.Resolve(_context, keywordInstance);
        
        result.Should().Be(EffectResult.Resolved);
    }
    
    [Test]
    public void Resolve_WhenFuncIsCalledWithCorrectOperands_ReturnsResult()
    {
        var sut = new EffectDefinition(ThreeOperandsFunc);
        var keywordInstance = Setup.KeywordInstance("ThreeOperandsFunc", Substitute.For<IAtom>(), "string", 1);
        
        var result = sut.Resolve(_context, keywordInstance);
        
        result.Should().Be(EffectResult.Resolved);
    }
    
    [Test]
    public void Resolve_WhenFuncIsCalledWithWrongOperandCount_ShouldThrowTargetParameterCountException()
    {
        var sut = new EffectDefinition(ThreeOperandsFunc);
        var keywordInstance = Setup.KeywordInstance("ThreeOperandsFunc", Substitute.For<IAtom>(), "string");
        
        var act = () => sut.Resolve(_context, keywordInstance);
        
        act.Should().Throw<TargetParameterCountException>();
    }
    
    [Test]
    public void Resolve_WhenFuncIsCalledWithOperandsOfWrongType_ShouldThrowArgumentException()
    {
        var sut = new EffectDefinition(ThreeOperandsFunc);
        var keywordInstance = Setup.KeywordInstance("ThreeOperandsFunc", 1, "string", Substitute.For<IAtom>());
        
        var act = () => sut.Resolve(_context, keywordInstance);
        
        act.Should().Throw<ArgumentException>();
    }
}