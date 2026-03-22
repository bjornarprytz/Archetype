using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Archetype.Core;

// ---------------------------------------------------------------------------
//  GameDefinitionJsonOptions — shared JsonSerializerOptions for GameDefinition
// ---------------------------------------------------------------------------

/// <summary>
/// Provides a pre-configured <see cref="JsonSerializerOptions"/> instance that
/// can serialise and deserialise a full <see cref="GameDefinition"/> including
/// its <see cref="KeywordNode"/> expression trees.
/// <para>
/// Background: <see cref="Literal"/> carries both <c>[JsonConverter(typeof(LiteralConverter))]</c>
/// and is a <c>[JsonDerivedType]</c> on <see cref="KeywordNode"/>.  In .NET 8+, the
/// <c>[JsonDerivedType]</c> polymorphic resolver does not honour the per-type
/// <c>[JsonConverter]</c> on a derived class.  The workaround is to register a
/// <see cref="KeywordNodeConverter"/> that handles the full union including
/// <see cref="Literal"/>.
/// </para>
/// </summary>
public static class GameDefinitionJsonOptions
{
    private static readonly JsonSerializerOptions _instance = Build();

    /// <summary>
    /// Returns a shared, frozen <see cref="JsonSerializerOptions"/> configured
    /// for <see cref="GameDefinition"/> round-trips.
    /// </summary>
    public static JsonSerializerOptions Instance => _instance;

    /// <summary>
    /// Builds a new <see cref="JsonSerializerOptions"/> with all converters
    /// required for <see cref="GameDefinition"/> serialisation.
    /// </summary>
    public static JsonSerializerOptions Build(bool writeIndented = true)
    {
        var opts = new JsonSerializerOptions
        {
            WriteIndented = writeIndented,
            // Strip [JsonDerivedType] polymorphism from KeywordNode so that
            // KeywordNodeConverter can take full control of the union.
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { StripKeywordNodePolymorphism },
            },
        };
        // Register AFTER setting TypeInfoResolver so converter is in scope.
        opts.Converters.Add(new KeywordNodeConverter());
        return opts;
    }

    /// <summary>
    /// Removes the <see cref="JsonPolymorphismOptions"/> from <see cref="KeywordNode"/>
    /// so that <see cref="KeywordNodeConverter"/> registered in
    /// <see cref="JsonSerializerOptions.Converters"/> takes over the union.
    /// Without this, <c>[JsonDerivedType]</c> and <c>Converters</c> conflict.
    /// </summary>
    private static void StripKeywordNodePolymorphism(
        System.Text.Json.Serialization.Metadata.JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type == typeof(KeywordNode))
            typeInfo.PolymorphismOptions = null;
    }
}

// ---------------------------------------------------------------------------
//  KeywordNodeConverter — handles KeywordNode union including Literal (D2)
// ---------------------------------------------------------------------------

/// <summary>
/// Custom <see cref="JsonConverter{T}"/> for <see cref="KeywordNode"/> that
/// handles the full discriminated union:
/// <see cref="ParameterRef"/> (<c>"ref"</c>),
/// <see cref="Literal"/> (<c>"lit"</c>),
/// <see cref="Invocation"/> (<c>"inv"</c>).
/// <para>
/// This converter is needed because the default <c>[JsonDerivedType]</c>
/// polymorphic path does not support derived types that carry
/// <c>[JsonConverter]</c> (e.g. <see cref="Literal"/>).
/// </para>
/// </summary>
public sealed class KeywordNodeConverter : JsonConverter<KeywordNode>
{
    private static readonly LiteralConverter _literalConverter = new();
    private const string DiscriminatorKey = "$type";

    /// <inheritdoc />
    public override KeywordNode? Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Buffer the whole object so we can look at $type first.
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty(DiscriminatorKey, out var typeProp))
            throw new JsonException("KeywordNode JSON missing '$type' discriminator.");

        var discriminator = typeProp.GetString();
        var rawJson       = root.GetRawText();

        return discriminator switch
        {
            "ref" => JsonSerializer.Deserialize<ParameterRef>(rawJson, options),
            "inv" => DeserializeInvocation(root, options),
            "lit" => DeserializeLiteral(root, options),
            _     => throw new JsonException($"Unknown KeywordNode discriminator '{discriminator}'."),
        };
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer, KeywordNode value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case ParameterRef pr:
                writer.WriteStartObject();
                writer.WriteString(DiscriminatorKey, "ref");
                writer.WriteString("Name", pr.Name);
                writer.WriteEndObject();
                break;

            case Literal lit:
                // Write Literal using its dedicated converter but prepend $type.
                writer.WriteStartObject();
                writer.WriteString(DiscriminatorKey, "lit");
                // Delegate inner value fields to LiteralConverter by writing a
                // temporary sub-document and splicing it in.
                using (var ms = new System.IO.MemoryStream())
                {
                    using var tmpWriter = new Utf8JsonWriter(ms);
                    _literalConverter.Write(tmpWriter, lit, options);
                    tmpWriter.Flush();
                    // Parse the {"t":...,"v":...} object and copy its properties.
                    var litDoc = JsonDocument.Parse(ms.ToArray());
                    foreach (var prop in litDoc.RootElement.EnumerateObject())
                        prop.WriteTo(writer);
                }
                writer.WriteEndObject();
                break;

            case Invocation inv:
                writer.WriteStartObject();
                writer.WriteString(DiscriminatorKey, "inv");
                writer.WriteString("KeywordName", inv.KeywordName);
                writer.WritePropertyName("Args");
                writer.WriteStartArray();
                foreach (var arg in inv.Args)
                    Write(writer, arg, options);
                writer.WriteEndArray();
                writer.WriteEndObject();
                break;

            default:
                throw new JsonException($"Unknown KeywordNode type: {value.GetType().Name}");
        }
    }

    // JsonSerializer.Deserialize<> creates its own internal reader over the raw text,
    // already positioned at StartObject — no manual reader.Read() needed here.
    private static Invocation DeserializeInvocation(JsonElement root, JsonSerializerOptions options)
    {
        var kwName = root.GetProperty("KeywordName").GetString()
            ?? throw new JsonException("Invocation missing 'KeywordName'.");
        var args = new List<KeywordNode>();
        if (root.TryGetProperty("Args", out var argsEl))
            foreach (var argEl in argsEl.EnumerateArray())
            {
                var arg = JsonSerializer.Deserialize<KeywordNode>(argEl.GetRawText(), options);
                if (arg is not null) args.Add(arg);
            }
        return new Invocation(kwName, args.ToArray());
    }

    private static Literal DeserializeLiteral(JsonElement root, JsonSerializerOptions options)
    {
        // Re-serialize just the inner fields (without $type) and pass to LiteralConverter.
        using var ms = new System.IO.MemoryStream();
        using var tmpWriter = new Utf8JsonWriter(ms);
        tmpWriter.WriteStartObject();
        foreach (var prop in root.EnumerateObject())
            if (prop.Name != DiscriminatorKey)
                prop.WriteTo(tmpWriter);
        tmpWriter.WriteEndObject();
        tmpWriter.Flush();

        var bytes  = ms.ToArray();
        var reader = new Utf8JsonReader(bytes);
        reader.Read(); // advance to StartObject before delegating
        return _literalConverter.Read(ref reader, typeof(Literal), options)!;
    }
}
