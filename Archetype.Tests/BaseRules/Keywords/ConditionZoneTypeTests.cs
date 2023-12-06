using Archetype.Framework.BaseRules.Keywords.Primitive;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;
// ReSharper disable MemberCanBePrivate.Global

namespace Archetype.Tests.BaseRules;

[TestFixture]
public class ConditionZoneTypeTests
{
    public interface ISomeZone : IZone { }
    public interface ISomeOtherZone : IZone { }
    
    private ConditionZoneType<ISomeZone> _sut = default!;
    
    private IResolutionContext _context = default!;
    
    [SetUp]
    public void Setup()
    {
        _sut = new ();
        _context = Substitute.For<IResolutionContext>();
    }
    
    [Test]
    public void ShouldHaveCorrectName()
    {
        _sut.Name.Should().Be("CONDITION_ZONE_TYPE");
    }
    
    [Test]
    public void ShouldReturnTrue_WhenZoneIsCorrectType()
    {
        // Arrange
        _context.Source.CurrentZone.Returns(Substitute.For<ISomeZone>());
        
        // Act
        var result = _sut.Check(_context, Substitute.For<IKeywordInstance>());

        // Assert
        result.Should().BeTrue();
    }
    
    [Test]
    public void ShouldReturnFalse_WhenZoneIsNotCorrectType()
    {
        // Arrange
        _context.Source.CurrentZone.Returns(Substitute.For<ISomeOtherZone>());
        
        // Act
        var result = _sut.Check(_context, Substitute.For<IKeywordInstance>());

        // Assert
        result.Should().BeFalse();
    }
}