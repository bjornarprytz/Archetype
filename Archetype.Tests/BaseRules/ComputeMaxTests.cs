using Archetype.BasicRules.Primitives;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;
using FluentAssertions;
using NSubstitute;

namespace Archetype.Tests.BaseRules;

[TestFixture]
public class ComputeMaxTests
{
    private ComputeMax _sut = default!;
    
    
    [SetUp]
    public void SetUp()
    {
        _sut = new ComputeMax();
    }

    [Test]
    public void ShouldHaveCorrectName()
    {
        _sut.Name.Should().Be("COMPUTE_MAX");
    }

    [Test]
    public void ShouldComputeMax()
    {
        // Arrange
        var context = Substitute.For<IResolutionContext>();
        var keywordInstance = Substitute.For<IKeywordInstance>();
        var target1 = Substitute.For<ICard>();
        var target2 = Substitute.For<ICard>();
        var target3 = Substitute.For<ICard>();

        var char1 = Substitute.For<IKeywordInstance>();
        char1.Keyword.Returns("TestProperty");
        char1.Operands.Returns(Declare.Operands(Declare.Operand(1)));
        var char2 = Substitute.For<IKeywordInstance>();
        
        char2.Keyword.Returns("TestProperty");
        char2.Operands.Returns(Declare.Operands(Declare.Operand(2)));
        
        var char3 = Substitute.For<IKeywordInstance>();
        
        char3.Keyword.Returns("OtherProperty");
        char3.Operands.Returns(Declare.Operands(Declare.Operand(69)));

        keywordInstance.Operands.Returns(Declare.Operands(Declare.Operand("TestProperty")));
        keywordInstance.Targets.Returns(Declare.Targets(Declare.Target(target1), Declare.Target(target2), Declare.Target(target3)));
        
        target1.Characteristics.Returns( new Dictionary<string, IKeywordInstance> { ["TestProperty"] = char1 });
        target2.Characteristics.Returns( new Dictionary<string, IKeywordInstance> { ["TestProperty"] = char2 });
        target3.Characteristics.Returns( new Dictionary<string, IKeywordInstance> { ["OtherProperty"] = char3 });
        
        // Act
        
        var result = _sut.Compute(context, keywordInstance);
        
        // Assert
        
        result.Should().Be(2);
    }
    
    [Test]
    public void ShouldComputeMaxWithNoTargets()
    {
        // Arrange
        var context = Substitute.For<IResolutionContext>();
        var keywordInstance = Substitute.For<IKeywordInstance>();

        keywordInstance.Operands.Returns(Declare.Operands(Declare.Operand("TestProperty")));
        keywordInstance.Targets.Returns(Declare.Targets());
        
        // Act
        
        var result = _sut.Compute(context, keywordInstance);
        
        // Assert
        
        result.Should().Be(0);
    }
}