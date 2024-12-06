using Archetype.Framework.Parsing;
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
}