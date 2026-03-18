using System.Text.Json;
using Archetype.Core;

namespace Archetype.Tooling.Server.Handlers;

/// <summary>
/// Handles <c>UpdateField</c> — updates a non-DSL field on any entry
/// (name, textTemplate, signalBehaviour, static properties, etc.).
/// </summary>
public sealed class UpdateFieldHandler(SidecarState sidecar)
{
    /// <summary>
    /// Params: <c>entryKind</c>, <c>entryName</c>, <c>field</c>, <c>value</c>.
    /// </summary>
    public object Handle(JsonElement? p)
    {
        var kind  = p?.GetProperty("entryKind").GetString()
            ?? throw new ArgumentException("UpdateField: 'entryKind' required.");
        var name  = p?.GetProperty("entryName").GetString()
            ?? throw new ArgumentException("UpdateField: 'entryName' required.");
        var field = p?.GetProperty("field").GetString()
            ?? throw new ArgumentException("UpdateField: 'field' required.");

        if (!p!.Value.TryGetProperty("value", out var valueEl))
            throw new ArgumentException("UpdateField: 'value' required.");

        var state = sidecar.State;

        switch (kind.ToLowerInvariant())
        {
            case "keyword":
                if (state.Keywords.TryGetValue(name, out var kw))
                    ApplyKeywordField(kw, field, valueEl);
                break;

            case "card":
                if (state.Cards.TryGetValue(name, out var card))
                    ApplyCardField(card, field, valueEl);
                break;

            case "zone":
                if (state.Zones.TryGetValue(name, out var zone))
                    zone.StaticProperties[field] = ReadValue(valueEl);
                break;

            case "player":
                if (state.Players.TryGetValue(name, out var player))
                    player.StaticProperties[field] = ReadValue(valueEl);
                break;

            default:
                throw new ArgumentException($"UpdateField: unknown entryKind '{kind}'.");
        }

        return MutationHelpers.RevalidateAndBuildResponse(state, [name]);
    }

    private static void ApplyKeywordField(KeywordEntry kw, string field, JsonElement val)
    {
        switch (field)
        {
            case "textTemplate":
                kw.TextTemplate = val.GetString();
                break;
            case "signalBehaviour":
                if (val.GetString() is { } sb &&
                    Enum.TryParse<SignalBehaviour>(sb, out var b))
                    kw.SignalBehaviour = b;
                break;
            // "noSignal" is the renderer-side boolean shorthand for signal suppression.
            // Map it to the canonical SignalBehaviour enum value.
            case "noSignal":
                kw.SignalBehaviour = val.ValueKind == JsonValueKind.True
                    ? SignalBehaviour.Suppress
                    : SignalBehaviour.Default;
                break;
            // "parameters" arrives as a JSON array of { name, type } objects.
            case "parameters":
                kw.Parameters = ParseParameterDecls(val);
                break;
            // "returnType" is the TypeName enum value as a string.
            case "returnType":
                if (val.GetString() is { } rtStr &&
                    Enum.TryParse<TypeName>(rtStr, ignoreCase: true, out var rt))
                    kw.ReturnType = rt;
                else
                    kw.ReturnType = null;
                break;
        }
    }

    /// <summary>
    /// Parses a JSON array of <c>{ name: string, type: string }</c> objects into
    /// a list of <see cref="ParameterDecl"/> values.
    /// Unknown type strings default to <see cref="TypeName.Atom"/>.
    /// </summary>
    private static List<ParameterDecl> ParseParameterDecls(JsonElement val)
    {
        var list = new List<ParameterDecl>();
        if (val.ValueKind != JsonValueKind.Array) return list;
        foreach (var el in val.EnumerateArray())
        {
            var name = el.TryGetString("name") ?? "";
            var typeName = TypeName.Atom;
            if (el.TryGetString("type") is { } typeStr &&
                Enum.TryParse<TypeName>(typeStr, ignoreCase: true, out var t))
                typeName = t;
            list.Add(new ParameterDecl(name, typeName));
        }
        return list;
    }

    private static void ApplyCardField(CardEntry card, string field, JsonElement val)
    {
        switch (field)
        {
            case "flavourText":
                card.FlavourText = val.GetString();
                break;
            case "artPath":
                card.ArtPath = val.GetString();
                break;
            default:
                card.StaticProperties[field] = ReadValue(val);
                break;
        }
    }

    private static object ReadValue(JsonElement el) => el.ValueKind switch
    {
        JsonValueKind.Number  => el.GetDouble(),
        JsonValueKind.True    => true,
        JsonValueKind.False   => false,
        JsonValueKind.String  => el.GetString() ?? "",
        _                     => el.ToString(),
    };
}
