using Archetype.Framework.BaseRules.Keywords.Primitive;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;
// ReSharper disable MemberCanBePrivate.Global

namespace Archetype.Tests.BaseRules;

[TestFixture]
public class ConditionZoneTypeTests
{
    private ConditionZoneType _sut = default!;
    
    private IResolutionContext _context = default!;
    
    [SetUp]
    public void Setup()
    {
        _sut = new ConditionZoneType();
        _context = Substitute.For<IResolutionContext>();
    }
    
    [Test]
    public void ShouldReturnTrue_WhenZoneIsCorrectType()
    {
        // Arrange
        var zone = Substitute.For<IZone>();
            zone.SetupCharacteristicReturn("TYPE", "SOME_TYPE");
        var atom = Substitute.For<IAtom>();
        atom.CurrentZone.Returns(zone);
        
        var keywordInstance = _sut
            .CreateInstance(atom, "SOME_TYPE");
        
        // Act
        var result = _sut.Check(_context, keywordInstance);

        // Assert
        result.Should().BeTrue();
    }
    
    [Test]
    public void ShouldReturnFalse_WhenZoneIsNotCorrectType()
    {
        // Arrange
        var zone = Substitute.For<IZone>();
            zone.SetupCharacteristicReturn("TYPE", "SOME_OTHER_TYPE");
        var atom = Substitute.For<IAtom>();
        atom.CurrentZone.Returns(zone);
        
        var keywordInstance = _sut
            .CreateInstance(atom, "SOME_TYPE");
        
            
        // Act
        var result = _sut.Check(_context, keywordInstance);

        // Assert
        result.Should().BeFalse();
    }
}