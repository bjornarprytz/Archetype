using Archetype.Framework.BaseRules.Keywords;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;

namespace Archetype.Tests.BaseRules;

[TestFixture]
public class ChangeZoneTests
{
    private IResolutionContext _context = default!;
    private IAtom _atom = default!;
    private IZone _from = default!;
    private IZone _to = default!;
    
    
    [SetUp]
    public void Setup()
    {
        _context = Substitute.For<IResolutionContext>();
        _atom = Substitute.For<IAtom>();
        _from = Substitute.For<IZone>();
        _to = Substitute.For<IZone>();
    }
    
    [Test]
    public void ShouldChangeZone()
    {
        // Arrange
        _atom.CurrentZone.Returns(_from);
        
        // Act
        var result = Effects.ChangeZone(_context, _atom, _to);
        
        // Assert
        
        _from.Received(1).Remove(_atom);
        _to.Received(1).Add(_atom);
        _atom.Received(1).CurrentZone = _to;
        result.Should().BeOfType<EffectResult>();
    }
    
    [Test]
    public void FromIsNull_ShouldChangeZone()
    {
        // Arrange
        _atom.CurrentZone.Returns((IZone?) null);
        
        // Act
        var result = Effects.ChangeZone(_context, _atom, _to);
        
        // Assert
        
        _to.Received(1).Add(_atom);
        _atom.Received(1).CurrentZone = _to;
        result.Should().BeOfType<EffectResult>();
    }
}