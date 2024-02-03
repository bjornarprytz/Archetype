using Archetype.Framework.BaseRules.Keywords;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;

namespace Archetype.Tests.BaseRules;

[TestFixture]
public class PromptTests
{
    private IResolutionContext _context = default!;
    
    [SetUp]
    public void Setup()
    {
        _context = Substitute.For<IResolutionContext>();
    }
    
    [Test]
    public void PickBetweenNandM_ShouldReturnCorrectPromptDescription()
    {
        // Arrange

        var options = new[]
        {
            Substitute.For<IAtom>(),
            Substitute.For<IAtom>(),
            Substitute.For<IAtom>(),
        };
        var atomProvider = Substitute.For<IAtomProvider>();
        atomProvider.ProvideAtoms(_context).Returns(options);
        
        var promptGuid = Guid.NewGuid();

        // Act
        
        var result = Prompt.PickBetweenNandM(_context, promptGuid, atomProvider, 1, 2, "Pick one or two things among the options");

        // Assert
        
        result.PromptText.Should().Be("Pick one or two things among the options");
        result.PromptId.Should().Be(promptGuid);
        result.MinPicks.Should().Be(1);
        result.MaxPicks.Should().Be(2);
        result.Options.Should().BeEquivalentTo(options.Select(a => a.Id));
    }
    
    [Test]
    public void PickN_ShouldReturnCorrectPromptDescription()
    {
        // Arrange

        var options = new[]
        {
            Substitute.For<IAtom>(),
            Substitute.For<IAtom>(),
            Substitute.For<IAtom>(),
        };
        var atomProvider = Substitute.For<IAtomProvider>();
        atomProvider.ProvideAtoms(_context).Returns(options);
        
        var promptGuid = Guid.NewGuid();

        // Act
        
        var result = Prompt.PickN(_context, promptGuid, atomProvider, 2, "Pick two things among the options");

        // Assert
        
        result.PromptText.Should().Be("Pick two things among the options");
        result.PromptId.Should().Be(promptGuid);
        result.MinPicks.Should().Be(2);
        result.MaxPicks.Should().Be(2);
        result.Options.Should().BeEquivalentTo(options.Select(a => a.Id));
    }
    
    [Test]
    public void PickOne_ShouldReturnCorrectPromptDescription()
    {
        // Arrange

        var options = new[]
        {
            Substitute.For<IAtom>(),
            Substitute.For<IAtom>(),
            Substitute.For<IAtom>(),
        };
        var atomProvider = Substitute.For<IAtomProvider>();
        atomProvider.ProvideAtoms(_context).Returns(options);
        
        var promptGuid = Guid.NewGuid();

        // Act
        
        var result = Prompt.PickOne(_context, promptGuid, atomProvider, "Pick one thing among the options");

        // Assert
        
        result.PromptText.Should().Be("Pick one thing among the options");
        result.PromptId.Should().Be(promptGuid);
        result.MinPicks.Should().Be(1);
        result.MaxPicks.Should().Be(1);
        result.Options.Should().BeEquivalentTo(options.Select(a => a.Id));
    }

    
}