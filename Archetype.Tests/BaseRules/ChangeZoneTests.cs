using Archetype.BasicRules.Primitives;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;
using FluentAssertions;
using NSubstitute;

namespace Archetype.Tests.BaseRules;

[TestFixture]
public class ChangeZoneTests
{
    private ChangeZone _sut = default!;
    
    private ICard _card = default!;
    private IZone _from = default!;
    private IZone _to = default!;
    
    
    [SetUp]
    public void Setup()
    {
        _sut = new ChangeZone();
        
        _card = Substitute.For<ICard>();
        _from = Substitute.For<IZone>();
        _to = Substitute.For<IZone>();
    }
    
    [Test]
    public void ShouldHaveKeywordName()
    {
        _sut.Name.Should().Be("CHANGE_ZONE");
    }

    [Test]
    public void ShouldChangeZone()
    {
        // Arrange
        _card.CurrentZone.Returns(_from);
        _from.Atoms.Returns(new List<IAtom> { _card });

        var payload = new EffectPayload(
            Guid.NewGuid(),
            Substitute.For<IAtom>(),
            _sut.Name,
            new object[] { },
            new IAtom[] { _card, _to }
        );

        // Act
        var result = _sut.Resolve(Substitute.For<IResolutionContext>(), payload);

        // Assert
        _card.CurrentZone.Should().Be(_to);
        _from.Received().Remove(_card);
        _to.Received().Add(_card);
        result.Should().BeOfType<ChangeZoneEvent>();
        result.As<ChangeZoneEvent>().Card.Should().Be(_card);
        result.As<ChangeZoneEvent>().From.Should().Be(_from);
        result.As<ChangeZoneEvent>().To.Should().Be(_to);
        result.As<ChangeZoneEvent>().Source.Should().Be(payload.Source);
    }
    
    [Test]
    public void FromIsNull_ShouldChangeZone()
    {
        // Arrange
        _card.CurrentZone.Returns((IZone?)null);

        var payload = new EffectPayload(
            Guid.NewGuid(),
            Substitute.For<IAtom>(),
            _sut.Name,
            new object[] { },
            new IAtom[] { _card, _to }
        );

        // Act
        var result = _sut.Resolve(Substitute.For<IResolutionContext>(), payload);

        // Assert
        _card.CurrentZone.Should().Be(_to);
        _to.Received().Add(_card);
        result.Should().BeOfType<ChangeZoneEvent>();
        result.As<ChangeZoneEvent>().Card.Should().Be(_card);
        result.As<ChangeZoneEvent>().From.Should().Be(null);
        result.As<ChangeZoneEvent>().To.Should().Be(_to);
        result.As<ChangeZoneEvent>().Source.Should().Be(payload.Source);
    }
}