using Archetype.Framework.BaseRules.Keywords.Primitive;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;

namespace Archetype.Tests.BaseRules;

[TestFixture]
public class ChangeZoneTests
{
    private ChangeZone _sut = default!;
    
    private IAtom _atom = default!;
    private IZone _from = default!;
    private IZone _to = default!;
    
    
    [SetUp]
    public void Setup()
    {
        _sut = new ChangeZone();
        
        _atom = Substitute.For<IAtom>();
        _from = Substitute.For<IZone>();
        _to = Substitute.For<IZone>();
    }
    
    [Test]
    public void ShouldChangeZone()
    {
        // Arrange
        _atom.CurrentZone.Returns(_from);
        _from.Atoms.Returns(new List<IAtom> { _atom });
        
        var resolutionContext = Substitute.For<IResolutionContext>();

        var payload = _sut.CreateInstance(_atom, _to).BindPayload(resolutionContext);

        // Act
        var result = _sut.Resolve(resolutionContext, payload);

        // Assert
        _atom.CurrentZone.Should().Be(_to);
        _from.Received().Remove(_atom);
        _to.Received().Add(_atom);
        result.Should().BeOfType<ChangeZoneEvent>();
        result.As<ChangeZoneEvent>().Atom.Should().Be(_atom);
        result.As<ChangeZoneEvent>().From.Should().Be(_from);
        result.As<ChangeZoneEvent>().To.Should().Be(_to);
        result.As<ChangeZoneEvent>().Source.Should().Be(payload.Source);
    }
    
    [Test]
    public void FromIsNull_ShouldChangeZone()
    {
        // Arrange
        _atom.CurrentZone.Returns((IZone?)null);

        var resolutionContext = Substitute.For<IResolutionContext>();

        var payload = _sut.CreateInstance(_atom, _to).BindPayload(resolutionContext);

        // Act
        var result = _sut.Resolve(resolutionContext, payload);

        // Assert
        _atom.CurrentZone.Should().Be(_to);
        _to.Received().Add(_atom);
        result.Should().BeOfType<ChangeZoneEvent>();
        result.As<ChangeZoneEvent>().Atom.Should().Be(_atom);
        result.As<ChangeZoneEvent>().From.Should().Be(null);
        result.As<ChangeZoneEvent>().To.Should().Be(_to);
        result.As<ChangeZoneEvent>().Source.Should().Be(payload.Source);
    }
}