using Archetype.Framework.BaseRules.Keywords;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.Meta;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;

namespace Archetype.Tests.BaseRules;

[TestFixture]
public class ChangeStateTests
{
    private IAtom _target = default!;
    
    [SetUp]
    public void Setup()
    {
        _target = Substitute.For<IAtom>();
    }
    
    [Test]
    public void ShouldChangeState()
    {
        // Arrange
        var context = Substitute.For<IResolutionContext>();

        
        var result = Effects.ChangeState(context, _target, "DUMMY", 42);

        // Assert
        _target.Received(1).SetState<int>("DUMMY", 42);
        result.Should().BeOfType<EffectResult>();
    }
    
    [Test]
    public void StateUnchanged_ReturnsNoOp()
    {
        // Arrange
        var context = Substitute.For<IResolutionContext>();
        
        _target.GetState<int>("DUMMY").Returns(42);
        
        // Act
        var result = Effects.ChangeState(context, _target, "DUMMY", 42);
        
        // Assert
        _target.DidNotReceive().SetState<int>("DUMMY", 42);
        result.Should().BeOfType<NoOpResult>();
    }
}