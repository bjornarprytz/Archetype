using Archetype.Framework.BaseRules.Keywords.Primitive;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;

namespace Archetype.Tests.BaseRules;

[TestFixture]
public class PromptTests
{
    
    private Prompt _sut = default!;
    
    private IResolutionContext _context = default!;
    
    [SetUp]
    public void Setup()
    {
        _sut = new Prompt();
        _context = Substitute.For<IResolutionContext>();
    }
    
    [Test]
    public void ShouldHaveCorrectName()
    {
        _sut.Name.Should().Be("PROMPT");
    }
    
    [Test]
    public void ShouldReturnEvent()
    {
        // Arrange

        var options = new IAtom[]
        {
            Substitute.For<IAtom>(),
            Substitute.For<IAtom>(),
            Substitute.For<IAtom>(),
        };
        
        var payload = new EffectPayload(
            Guid.NewGuid(),
            Substitute.For<IAtom>(),
            _sut.Name,
            new object[] { options, 1, 2, "Pick one or two things among the options" },
            Array.Empty<IAtom>()
        );

        // Act
        var result = _sut.Resolve(_context, payload);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<PromptEvent>();
        result.As<PromptEvent>().PromptId.Should().Be(payload.Id);
        result.As<PromptEvent>().Options.Should().BeEquivalentTo(options.Select(o => o.Id));
        result.As<PromptEvent>().MinPicks.Should().Be(1);
        result.As<PromptEvent>().MaxPicks.Should().Be(2);
        result.As<PromptEvent>().PromptText.Should().Be("Pick one or two things among the options");
        result.Source.Should().Be(payload.Source);
    }

    
}