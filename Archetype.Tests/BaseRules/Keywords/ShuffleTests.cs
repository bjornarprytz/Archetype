using Archetype.Framework.BaseRules.Keywords;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;

namespace Archetype.Tests.BaseRules;

[TestFixture]
public class ShuffleTests
{
    private IOrderedZone _targetZone = default!;
    private IResolutionContext _context = default!;
    
    [SetUp]
    public void Setup()
    {
        _targetZone = Substitute.For<IOrderedZone>();
        _context = Substitute.For<IResolutionContext>();
    }
    
    [Test]
    public void ShouldShuffleZone()
    {
        // Act
        var result = Effects.Shuffle(_context, _targetZone);

        // Assert
        _targetZone.Received().Shuffle();
        result.Should().BeOfType<EffectResult>();
    }
}