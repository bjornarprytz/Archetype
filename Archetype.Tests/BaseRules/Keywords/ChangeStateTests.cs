using Archetype.Framework.BaseRules.Keywords.Primitive;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Meta;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;

namespace Archetype.Tests.BaseRules;

[TestFixture]
public class ChangeStateTests
{
    private DummyChangeState _sut = default!;
    
    private IAtom _target = default!;
    
    [SetUp]
    public void Setup()
    {
        _sut = new DummyChangeState();
        _target = Substitute.For<IAtom>();
    }
    
    [Test]
    public void ShouldChangeState()
    {
        // Arrange
        var payload = new EffectPayload(
            Guid.NewGuid(),
            Substitute.For<IAtom>(),
            _sut.Name,
            new object?[] { _target, 42 }
        );

        // Act
        var result = _sut.Resolve(Substitute.For<IResolutionContext>(), payload);

        // Assert
        _target.State["DUMMY"].Should().Be(42);
        result.Should().BeOfType<ChangeStateEvent<int>>();
        result.As<ChangeStateEvent<int>>().Atom.Should().Be(_target);
        result.As<ChangeStateEvent<int>>().Property.Should().Be("DUMMY");
        result.As<ChangeStateEvent<int>>().Value.Should().Be(42);
        result.As<ChangeStateEvent<int>>().Source.Should().Be(payload.Source);
    }
    
    [Test]
    public void StateUnchanged_ReturnsNonEvent()
    {
        // Arrange
        var payload = new EffectPayload(
            Guid.NewGuid(),
            Substitute.For<IAtom>(),
            _sut.Name,
            new object[] { _target, 1 }
        );

        _target.State.Returns(new Dictionary<string, object> { { "DUMMY", 1 } });

        // Act
        var result = _sut.Resolve(Substitute.For<IResolutionContext>(), payload);

        // Assert
        _target.State["DUMMY"].Should().Be(1);
        result.Should().BeOfType<NonEvent>();
        result.As<NonEvent>().Source.Should().Be(payload.Source);
    }


    [Keyword("DUMMY")]
    private class DummyChangeState : ChangeState<IAtom, int>
    {
        public override string ReminderText => "Dummy reminder text";
        protected override string Property => "DUMMY";
    }
}