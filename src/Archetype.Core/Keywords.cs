using System.Text.Json;
using System.Text.Json.Serialization;

namespace Archetype.Core;

// ---------------------------------------------------------------------------
//  KeywordNode — immutable expression tree (D2)
// ---------------------------------------------------------------------------

/// <summary>
/// Base type for all nodes in a keyword expression tree.  The same tree
/// drives both execution (via the interpreter in <c>Archetype.Engine</c>) and
/// text rendering (via <c>TextRenderer</c> in <c>Archetype.Text</c>), satisfying
/// the dual-use invariant in §1.1.
/// </summary>
[JsonDerivedType(typeof(ParameterRef), typeDiscriminator: "ref")]
[JsonDerivedType(typeof(Literal),      typeDiscriminator: "lit")]
[JsonDerivedType(typeof(Invocation),   typeDiscriminator: "inv")]
public abstract record KeywordNode;

/// <summary>
/// Refers to a declared parameter by name.  Resolves to its bound value in
/// the current <c>Bindings</c> during execution, or renders as the parameter
/// label during definition-time text rendering.
/// </summary>
/// <param name="Name">The declared parameter name to look up.</param>
public sealed record ParameterRef(string Name) : KeywordNode;

/// <summary>
/// A constant value hardcoded in the keyword definition.  Can hold a
/// <see cref="double"/>, <see cref="bool"/>, <see cref="string"/>, or an
/// <see cref="AtomId"/> referencing a known atom.
/// </summary>
/// <param name="Value">The literal value.</param>
[JsonConverter(typeof(LiteralConverter))]
public sealed record Literal(object Value) : KeywordNode;

// ---------------------------------------------------------------------------
//  LiteralConverter — custom JSON converter for Literal.Value : object
// ---------------------------------------------------------------------------

/// <summary>
/// Custom <see cref="JsonConverter{T}"/> for <see cref="Literal"/>.
/// <para>
/// <see cref="Literal.Value"/> is typed as <c>object</c> but in practice
/// holds one of: <c>double</c>, <c>bool</c>, <c>string</c>, or
/// <see cref="AtomId"/>.  This converter writes a discriminated JSON object
/// so that round-trips preserve the concrete type.
/// </para>
/// </summary>
internal sealed class LiteralConverter : JsonConverter<Literal>
{
    // Tag names used in JSON.
    private const string TypeTag  = "t";
    private const string ValueTag = "v";

    public override Literal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject for Literal.");

        string? tag   = null;
        object? value = null;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName) continue;
            var propName = reader.GetString();
            reader.Read();

            if (propName == TypeTag)
                tag = reader.GetString();
            else if (propName == ValueTag)
            {
                // Defer value parsing until we know the type tag.
                // Use a raw value to re-parse after we've seen the tag.
                // Since JSON objects have no guaranteed property order, we
                // read both properties in one pass by capturing a JsonElement.
                value = JsonElement.ParseValue(ref reader);
            }
        }

        if (tag is null)
            throw new JsonException("Literal JSON missing 't' (type tag).");

        var element = (JsonElement)(value ?? throw new JsonException("Literal JSON missing 'v' (value)."));
        object boxed = tag switch
        {
            "d"    => (object)element.GetDouble(),
            "b"    => element.GetBoolean(),
            "s"    => element.GetString() ?? string.Empty,
            "atom" => new AtomId(element.GetInt64()),
            _      => throw new JsonException($"Unknown Literal type tag '{tag}'.")
        };

        return new Literal(boxed);
    }

    public override void Write(Utf8JsonWriter writer, Literal value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(TypeTag, GetTag(value.Value));
        writer.WritePropertyName(ValueTag);
        WriteValue(writer, value.Value);
        writer.WriteEndObject();
    }

    private static string GetTag(object v) => v switch
    {
        double   => "d",
        bool     => "b",
        string   => "s",
        AtomId   => "atom",
        _        => throw new JsonException(
                        $"Literal holds unsupported type {v?.GetType().Name ?? "null"}. " +
                        "Allowed: double, bool, string, AtomId.")
    };

    private static void WriteValue(Utf8JsonWriter writer, object v)
    {
        switch (v)
        {
            case double d:  writer.WriteNumberValue(d); break;
            case bool   b:  writer.WriteBooleanValue(b); break;
            case string s:  writer.WriteStringValue(s); break;
            case AtomId a:  writer.WriteNumberValue(a.Value); break;
            default:
                throw new JsonException(
                    $"Literal holds unsupported type {v?.GetType().Name ?? "null"}.");
        }
    }
}

/// <summary>
/// Calls another keyword (built-in or game-creator-defined) with the given
/// argument nodes.  The keyword name is looked up in
/// <c>GameDefinition.Keywords</c> at execution time.
/// </summary>
/// <param name="KeywordName">Name of the keyword to invoke.</param>
/// <param name="Args">Argument expression nodes.</param>
public sealed record Invocation(string KeywordName, params KeywordNode[] Args) : KeywordNode;

// ---------------------------------------------------------------------------
//  ParameterDecl — typed parameter declaration (D2 / A14)
// ---------------------------------------------------------------------------

/// <summary>
/// The engine's type vocabulary for keyword parameters.  <see cref="Atom"/>
/// is the generic atom type; <see cref="Card"/>, <see cref="Zone"/>,
/// <see cref="Player"/>, and <see cref="Session"/> are atom subtypes that
/// trigger stricter authoring-time checks.
/// </summary>
public enum TypeName
{
    /// <summary>Generic atom reference (any <see cref="AtomKind"/>).</summary>
    Atom,
    /// <summary>Atom restricted to <see cref="AtomKind.Card"/>.</summary>
    Card,
    /// <summary>Atom restricted to <see cref="AtomKind.Zone"/>.</summary>
    Zone,
    /// <summary>Atom restricted to <see cref="AtomKind.Player"/>.</summary>
    Player,
    /// <summary>Atom restricted to <see cref="AtomKind.Session"/>.</summary>
    Session,
    /// <summary>Numeric value (double).</summary>
    Number,
    /// <summary>Boolean value.</summary>
    Boolean,
    /// <summary>Name of a named condition.</summary>
    ConditionName,
    /// <summary>Name of a property/accumulator.</summary>
    PropertyName,
    /// <summary>An opaque contribution ID returned by mutation primitives.</summary>
    ContributionId,
    /// <summary>A <see cref="LifetimeSpec"/> data value.</summary>
    Lifetime,
    /// <summary>An <see cref="EffectBlockDef"/> passed as a parameter.</summary>
    EffectBlock,
    /// <summary>
    /// A named card definition — validated at load time against
    /// <c>GameDefinition.CardDefinitions</c>.
    /// </summary>
    CardDefinitionName,
    /// <summary>
    /// A named zone definition — validated at load time against
    /// <c>GameDefinition.ZoneDefinitions</c>.
    /// </summary>
    ZoneDefinitionName,
    /// <summary>
    /// A reference to a <see cref="GameEvent"/> node in the event log,
    /// accessible through <c>event-arg</c>.
    /// </summary>
    EventRef,
    /// <summary>An ordered collection of atoms (returned by <c>shuffle</c>, etc.).</summary>
    Collection,
}

/// <summary>
/// Declares a single parameter on a <see cref="KeywordDefinition"/>.
/// <para>
/// Built-in keywords may additionally specify <see cref="AtomKindRestriction"/>
/// to constrain a generic <c>Atom</c>-typed parameter to a subset of atom
/// kinds.  Game-creator-defined keywords may not set this field.
/// </para>
/// </summary>
/// <param name="Name">The parameter name used in <c>ParameterRef</c> nodes.</param>
/// <param name="Type">The declared type of this parameter.</param>
/// <param name="AtomKindRestriction">
/// Optional restriction on atom kind.  When non-null, the type-checker at
/// authoring time requires the argument to resolve statically to one of the
/// listed atom kinds.
/// </param>
public sealed record ParameterDecl(
    string Name,
    TypeName Type,
    AtomKind[]? AtomKindRestriction = null);

// ---------------------------------------------------------------------------
//  KeywordDefinition — a complete keyword (D2)
// ---------------------------------------------------------------------------

/// <summary>
/// A complete keyword definition.  Used for both primitive (built-in) and
/// composite (game-creator-defined) keywords.
/// <para>
/// Primitive keywords carry a <c>null</c> <see cref="Body"/> with a sentinel
/// <see cref="PrimitiveSentinel"/> marking the engine handler name.  Composite
/// keywords carry a real <see cref="Body"/> expression tree and
/// <c>null</c> sentinel.
/// </para>
/// </summary>
public sealed record KeywordDefinition(
    string Name,
    ParameterDecl[] Parameters,
    TypeName ReturnType,
    string Description,
    KeywordNode? Body = null,
    string? PrimitiveSentinel = null,
    string? TextTemplate = null)
{
    /// <summary>
    /// Returns <c>true</c> when this definition represents an engine-level
    /// mutation primitive rather than a game-creator-defined composite.
    /// </summary>
    public bool IsPrimitive => PrimitiveSentinel is not null;
}

// ---------------------------------------------------------------------------
//  EffectBlockDef and EffectBlockStep (D3 addendum)
// ---------------------------------------------------------------------------

/// <summary>
/// An ordered sequence of keyword invocations that executes atomically.
/// No triggers or state-based effects resolve mid-block.
/// </summary>
/// <param name="Steps">The steps to execute in order.</param>
public sealed record EffectBlockDef(IReadOnlyList<EffectBlockStep> Steps)
{
    /// <summary>An empty effect block (no-op).</summary>
    public static readonly EffectBlockDef Empty =
        new(Array.Empty<EffectBlockStep>());
}

/// <summary>
/// A single step within an <see cref="EffectBlockDef"/>.  Specifies which
/// keyword to invoke, what argument expressions to pass, and an optional
/// name to which the return value is bound for later steps.
/// </summary>
/// <param name="KeywordName">Name of the keyword to invoke.</param>
/// <param name="ArgNodes">Argument expression nodes.</param>
/// <param name="BindTo">
/// Optional: if non-null, the return value of this step is stored in
/// <c>ExecutionContext.Bindings</c> under this name and is accessible to
/// subsequent steps in the same block via <see cref="ParameterRef"/>.
/// </param>
public sealed record EffectBlockStep(
    string KeywordName,
    KeywordNode[] ArgNodes,
    string? BindTo = null);
