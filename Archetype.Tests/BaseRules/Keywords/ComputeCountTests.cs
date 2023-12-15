using Archetype.Framework.BaseRules.Keywords.Primitive;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;

namespace Archetype.Tests.BaseRules;

[TestFixture]
public class ComputeCountTests
{
    private ComputeCount _sut = default!;
    
    
    [SetUp]
    public void SetUp()
    {
        _sut = new ComputeCount();
    }


    [Test]
    public void ShouldComputeCount()
    {
        // Arrange
        var context = Substitute.For<IResolutionContext>();
        var keywordInstance = Substitute.For<IKeywordInstance>();
        var target1 = Substitute.For<ICard>();
        var target2 = Substitute.For<ICard>();
        var target3 = Substitute.For<ICard>();
        
        var atomProvider = Substitute.For<IAtomProvider>();
        atomProvider.ProvideAtoms(context).Returns(new List<IAtom> { target1, target2, target3 });

        keywordInstance.Operands.Returns(Declare.Operands(Declare.Operand(atomProvider), Declare.Operand("TestProperty")));
        
        target1.SetupCharacteristicReturn("TestProperty", 1); 
        target2.SetupCharacteristicReturn("TestProperty", 2); 
        target3.SetupCharacteristicReturn("OtherProperty", 69);
        
        // Act
        
        var result = _sut.Compute(context, keywordInstance);
        
        // Assert
        
        result.Should().Be(2);
    }
    
    [Test]
    public void ShouldComputeCountWithNoTargets()
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