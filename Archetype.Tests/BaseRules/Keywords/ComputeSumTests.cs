using Archetype.Framework.BaseRules.Keywords.Primitive;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;

namespace Archetype.Tests.BaseRules;

[TestFixture]
public class ComputeSumTests
{
    private ComputeSum _sut = default!;
    
    
    [SetUp]
    public void SetUp()
    {
        _sut = new ComputeSum();
    }


    [Test]
    public void ShouldComputeSum()
    {
        // Arrange
        var context = Substitute.For<IResolutionContext>();
        var keywordInstance = Substitute.For<IKeywordInstance>();
        var target1 = Substitute.For<ICard>();
        var target2 = Substitute.For<ICard>();
        var target3 = Substitute.For<ICard>();
        
        var atomProvider = Substitute.For<IAtomProvider>();
        atomProvider.ProvideAtoms(context).Returns(new List<IAtom> { target1, target2, target3 });

        var char1 = Substitute.For<IKeywordInstance>();
        char1.Keyword.Returns("TestProperty");
        char1.Operands.Returns(Declare.Operands(Declare.Operand(1)));
        var char2 = Substitute.For<IKeywordInstance>();
        
        char2.Keyword.Returns("TestProperty");
        char2.Operands.Returns(Declare.Operands(Declare.Operand(2)));
        
        var char3 = Substitute.For<IKeywordInstance>();
        
        char3.Keyword.Returns("OtherProperty");
        char3.Operands.Returns(Declare.Operands(Declare.Operand(69)));

        keywordInstance.Operands.Returns(Declare.Operands(Declare.Operand(atomProvider), Declare.Operand("TestProperty")));
        
        target1.Characteristics.Returns( new Dictionary<string, IKeywordInstance> { ["TestProperty"] = char1 });
        target2.Characteristics.Returns( new Dictionary<string, IKeywordInstance> { ["TestProperty"] = char2 });
        target3.Characteristics.Returns( new Dictionary<string, IKeywordInstance> { ["OtherProperty"] = char3 });
        
        // Act
        
        var result = _sut.Compute(context, keywordInstance);
        
        // Assert
        
        result.Should().Be(3);
    }
    
    [Test]
    public void ShouldComputeSumWithNoTargets()
    {
        // Arrange
        var context = Substitute.For<IResolutionContext>();
        var keywordInstance = Substitute.For<IKeywordInstance>();

        var atomProvider = Substitute.For<IAtomProvider>();
        atomProvider.ProvideAtoms(context).Returns(Enumerable.Empty<IAtom>());
        
        keywordInstance.Operands.Returns(Declare.Operands(Declare.Operand(atomProvider), Declare.Operand("TestProperty")));
        
        // Act
        
        var result = _sut.Compute(context, keywordInstance);
        
        // Assert
        
        result.Should().Be(0);
    }
    
    
}