using System.Text.Json;
using Archetype.Core;
using Xunit;

namespace Archetype.Tests.Serialization;

/// <summary>
/// Covers <see cref="KeywordNodeConverter"/> and <see cref="LiteralConverter"/>
/// round-trips, including the DeserializeLiteral path that caused a regression
/// when a nested Literal inside an Invocation's args was not advanced to
/// StartObject before being handed to LiteralConverter.Read.
/// </summary>
public class KeywordNodeSerializationTests
{
    private static readonly JsonSerializerOptions Options = GameDefinitionJsonOptions.Build();

    // -----------------------------------------------------------------------
    //  Literal round-trips
    // -----------------------------------------------------------------------

    [Fact]
    public void Literal_String_RoundTrips()
    {
        KeywordNode node = new Literal("health");
        var json = JsonSerializer.Serialize(node, Options);
        var back = JsonSerializer.Deserialize<KeywordNode>(json, Options);
        Assert.Equal(node, back);
    }

    [Fact]
    public void Literal_Number_RoundTrips()
    {
        KeywordNode node = new Literal(-1.0);
        var json = JsonSerializer.Serialize(node, Options);
        var back = JsonSerializer.Deserialize<KeywordNode>(json, Options);
        Assert.Equal(node, back);
    }

    [Fact]
    public void Literal_Bool_RoundTrips()
    {
        KeywordNode node = new Literal(true);
        var json = JsonSerializer.Serialize(node, Options);
        var back = JsonSerializer.Deserialize<KeywordNode>(json, Options);
        Assert.Equal(node, back);
    }

    [Fact]
    public void Literal_AtomId_RoundTrips()
    {
        KeywordNode node = new Literal(new AtomId(42));
        var json = JsonSerializer.Serialize(node, Options);
        var back = JsonSerializer.Deserialize<KeywordNode>(json, Options);
        Assert.Equal(node, back);
    }

    // -----------------------------------------------------------------------
    //  Literal nested inside an Invocation (the regression case)
    // -----------------------------------------------------------------------

    [Fact]
    public void Invocation_WithStringLiteralArg_RoundTrips()
    {
        // This is the exact shape that triggered the bug:
        // KeywordNodeConverter saw $type=lit and called DeserializeLiteral
        // with a reader positioned at None rather than StartObject.
        KeywordNode node = new Invocation("take-damage", [new Literal("health")]);
        var json = JsonSerializer.Serialize(node, Options);
        var back = JsonSerializer.Deserialize<KeywordNode>(json, Options);
        Assert.Equivalent(node, back);
    }

    [Fact]
    public void Invocation_WithNumberLiteralArg_RoundTrips()
    {
        KeywordNode node = new Invocation("take-damage", [new Literal(-5.0)]);
        var json = JsonSerializer.Serialize(node, Options);
        var back = JsonSerializer.Deserialize<KeywordNode>(json, Options);
        Assert.Equivalent(node, back);
    }

    [Fact]
    public void Invocation_WithMixedArgs_RoundTrips()
    {
        KeywordNode node = new Invocation("attack",
        [
            new ParameterRef("target"),
            new Literal("health"),
            new Literal(10.0),
        ]);
        var json = JsonSerializer.Serialize(node, Options);
        var back = JsonSerializer.Deserialize<KeywordNode>(json, Options);
        Assert.Equivalent(node, back);
    }

    // -----------------------------------------------------------------------
    //  Direct JSON parsing — ensures the raw discriminated form is accepted
    // -----------------------------------------------------------------------

    [Fact]
    public void Literal_String_DeserializesFromRawJson()
    {
        var json = """{"$type":"lit","t":"s","v":"health"}""";
        var result = JsonSerializer.Deserialize<KeywordNode>(json, Options);
        Assert.Equal(new Literal("health"), result);
    }

    [Fact]
    public void Literal_Number_DeserializesFromRawJson()
    {
        var json = """{"$type":"lit","t":"d","v":-1}""";
        var result = JsonSerializer.Deserialize<KeywordNode>(json, Options);
        Assert.Equal(new Literal(-1.0), result);
    }
}
