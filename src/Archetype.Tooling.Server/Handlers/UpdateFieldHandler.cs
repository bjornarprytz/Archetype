using System.Text.Json;

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

        switch (kind)
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
        }
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
