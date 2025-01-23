using Archetype.Framework.Core;
using Archetype.Framework.Parsing;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;
using FluentAssertions;
using Xunit;

namespace Archetype.Tests;

public class ParsingExtensionsTests
{
    
    [Theory]
    [InlineData("path.to.value", null, false)]
    [InlineData("'word'", "word", true)]
    [InlineData("123", null, false)]
    public void ParseWordExpression(string input, string? expectedOutput, bool expectedResult)
    {
        var result = input.TryParseWord(out var output);
        
        output.Should().Be(expectedOutput);
        result.Should().Be(expectedResult);
    }
    
    [Theory]
    [InlineData(">", ComparisonOperator.GreaterThan)]
    [InlineData(">=", ComparisonOperator.GreaterThanOrEqual)]
    [InlineData("<", ComparisonOperator.LessThan)]
    [InlineData("<=", ComparisonOperator.LessThanOrEqual)]
    [InlineData("=", ComparisonOperator.Equal)]
    [InlineData("!=", ComparisonOperator.NotEqual)]
    [InlineData("has", ComparisonOperator.Contains)]
    [InlineData("!has", ComparisonOperator.NotContains)]
    public void ParseComparisonOperator(string input, ComparisonOperator expectedOutput)
    {
        var result = input.ParseComparisonOperator();
        
        result.Should().Be(expectedOutput);
    }
    
    [Theory]
    [InlineData(ComparisonOperator.GreaterThan, typeof(int), typeof(int))]
    [InlineData(ComparisonOperator.GreaterThanOrEqual, typeof(int), typeof(int))]
    [InlineData(ComparisonOperator.LessThan, typeof(int), typeof(int))]
    [InlineData(ComparisonOperator.LessThanOrEqual, typeof(int), typeof(int))]
    [InlineData(ComparisonOperator.Equal, typeof(int), typeof(int))]
    [InlineData(ComparisonOperator.Equal, typeof(string), typeof(string))]
    [InlineData(ComparisonOperator.NotEqual, typeof(int), typeof(int))]
    [InlineData(ComparisonOperator.NotEqual, typeof(string), typeof(string))]
    [InlineData(ComparisonOperator.Contains, typeof(string[]), typeof(string))]
    [InlineData(ComparisonOperator.NotContains, typeof(int[]), typeof(int))]
    public void ValidateOrThrow_CompatibleTypes(ComparisonOperator comparisonOperator, Type leftType, Type rightType)
    {
        var action = () => comparisonOperator.ValidateOrThrow(leftType, rightType);
        
        action.Should().NotThrow();
    }
    
    [Theory]
    [InlineData(ComparisonOperator.GreaterThan, typeof(int), typeof(string))]
    [InlineData(ComparisonOperator.GreaterThanOrEqual, typeof(int), typeof(string))]
    [InlineData(ComparisonOperator.LessThan, typeof(int), typeof(string))]
    [InlineData(ComparisonOperator.LessThanOrEqual, typeof(int), typeof(string))]
    [InlineData(ComparisonOperator.Equal, typeof(int), typeof(string))]
    [InlineData(ComparisonOperator.NotEqual, typeof(int), typeof(string))]
    [InlineData(ComparisonOperator.Contains, typeof(int), typeof(int))]
    [InlineData(ComparisonOperator.NotContains, typeof(string), typeof(string))]
    public void ValidateOrThrow_IncompatibleTypes(ComparisonOperator comparisonOperator, Type leftType, Type rightType)
    {
        var action = () => comparisonOperator.ValidateOrThrow(leftType, rightType);
        
        action.Should().Throw<InvalidOperationException>();
    }
    
    [Theory]
    [InlineData(null, null, false)]
    [InlineData(typeof(int), null, false)]
    [InlineData(null, typeof(int), false)]
    [InlineData(typeof(int[]), typeof(int), true)]
    [InlineData(typeof(string[]), typeof(string), true)]
    [InlineData(typeof(IAtom[]), typeof(IAtom), true)]
    [InlineData(typeof(IEnumerable<int>), typeof(int), true)]
    public void IsCollectionOf_ReturnsExpectedResult(Type? leftType, Type? rightType, bool expectedResult)
    {
        leftType.IsCollectionOf(rightType).Should().Be(expectedResult);
    }
    
    [Theory]
    [InlineData(1, ComparisonOperator.GreaterThan, 2, false)]
    [InlineData(1, ComparisonOperator.GreaterThan, 1, false)]
    [InlineData(1, ComparisonOperator.GreaterThan, 0, true)]
    [InlineData(1, ComparisonOperator.GreaterThanOrEqual, 2, false)]
    [InlineData(1, ComparisonOperator.GreaterThanOrEqual, 1, true)]
    [InlineData(1, ComparisonOperator.LessThan, 2, true)]
    [InlineData(1, ComparisonOperator.LessThan, 1, false)]
    [InlineData(1, ComparisonOperator.LessThan, 0, false)]
    [InlineData(1, ComparisonOperator.LessThanOrEqual, 2, true)]
    [InlineData(1, ComparisonOperator.LessThanOrEqual, 1, true)]
    [InlineData(1, ComparisonOperator.LessThanOrEqual, 0, false)]
    [InlineData(1, ComparisonOperator.Equal, 1, true)]
    [InlineData(1, ComparisonOperator.Equal, 0, false)]
    [InlineData(1, ComparisonOperator.NotEqual, 1, false)]
    [InlineData(1, ComparisonOperator.NotEqual, 0, true)]
    [InlineData("word", ComparisonOperator.Equal, "word", true)]
    [InlineData("word", ComparisonOperator.Equal, "other", false)]
    [InlineData("word", ComparisonOperator.NotEqual, "word", false)]
    [InlineData("word", ComparisonOperator.NotEqual, "other", true)]
    [InlineData(new[] { 1, 2, 3 }, ComparisonOperator.Contains, 2, true)]
    [InlineData(new[] { 1, 2, 3 }, ComparisonOperator.Contains, 4, false)]
    [InlineData(new[] { 1, 2, 3 }, ComparisonOperator.NotContains, 2, false)]
    [InlineData(new[] { 1, 2, 3 }, ComparisonOperator.NotContains, 4, true)]
    [InlineData(new[] { "hello", "world" }, ComparisonOperator.NotContains, "other", true)]
    [InlineData(new[] { "hello", "world" }, ComparisonOperator.NotContains, "world", false)]
    [InlineData(new[] { "hello", "world" }, ComparisonOperator.Contains, "world", true)]
    [InlineData(new[] { "hello", "world" }, ComparisonOperator.Contains, "wo", false)]
    public void Compare_ReturnsExpectedResult(object left, ComparisonOperator comparisonOperator, object right, bool expectedResult)
    {
        var result = comparisonOperator.Compare(left, right);
        
        result.Should().Be(expectedResult);
    }
    
}