using System.Collections;
using Archetype.Framework.BaseRules.Keywords;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.State;
using FluentAssertions;
using NSubstitute;

namespace Archetype.Tests.BaseRules;

[TestFixture]
public class ComputeMaxTests
{
    private IResolutionContext _context = default!;
    
    
    [SetUp]
    public void SetUp()
    {
        _context = Substitute.For<IResolutionContext>();
    }

    [Theory]
    [TestCaseSource(typeof(ComputeCaseWithTargetsProvider))]
    public void ComputeFunctionShouldReturnExpectedResult(Func<IResolutionContext, IAtomProvider, string, int> compute, int expectedResult)
    {
        // Arrange
        var target1 = Substitute.For<ICard>();
        var target2 = Substitute.For<ICard>();
        var target3 = Substitute.For<ICard>();
        
        var atomProvider = Substitute.For<IAtomProvider>();
        
        atomProvider.ProvideAtoms(_context).Returns(new List<IAtom> { target1, target2, target3 });
        
        target1.GetStat("TestProperty").Returns(1);
        target2.GetStat("TestProperty").Returns(2);
        target3.GetStat("OtherProperty").Returns(69);
        
        // Act
        
        var result = compute(_context, atomProvider, "TestProperty");
        
        // Assert
        
        result.Should().Be(expectedResult);
    }
    
    [Theory]
    [TestCaseSource(typeof(ComputeCaseWithoutTargetsProvider))]
    public void ShouldComputeWithoutTargets(Func<IResolutionContext, IAtomProvider, string, int> compute, int expectedResult)
    {
        // Arrange
        var atomProvider = Substitute.For<IAtomProvider>();
        
        atomProvider.ProvideAtoms(_context).Returns(new List<IAtom>());
        
        // Act
        var result = compute(_context, atomProvider, "TestProperty");
        
        // Assert
        
        result.Should().Be(expectedResult);
        
    }
    
    private class ComputeCaseWithTargetsProvider : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { (Func<IResolutionContext, IAtomProvider, string, int>)Compute.Max, 2 };
            yield return new object[] { (Func<IResolutionContext, IAtomProvider, string, int>)Compute.Min, 1 };
            yield return new object[] { (Func<IResolutionContext, IAtomProvider, string, int>)Compute.Sum, 3 };
            yield return new object[] { (Func<IResolutionContext, IAtomProvider, string, int>)Compute.Count, 2 };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    
    private class ComputeCaseWithoutTargetsProvider : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { (Func<IResolutionContext, IAtomProvider, string, int>)Compute.Max, 0 };
            yield return new object[] { (Func<IResolutionContext, IAtomProvider, string, int>)Compute.Min, 0 };
            yield return new object[] { (Func<IResolutionContext, IAtomProvider, string, int>)Compute.Sum, 0 };
            yield return new object[] { (Func<IResolutionContext, IAtomProvider, string, int>)Compute.Count, 0 };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}