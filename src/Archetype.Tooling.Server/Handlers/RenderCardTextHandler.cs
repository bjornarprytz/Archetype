using System.Text.Json;
using Archetype.Core;
using Archetype.Text;

namespace Archetype.Tooling.Server.Handlers;

/// <summary>
/// Handles <c>RenderCardText</c> — renders a card's primary effect to a
/// <see cref="RenderNode"/> tree using <see cref="TextRenderer"/> (D28).
/// </summary>
public sealed class RenderCardTextHandler(SidecarState sidecar)
{
    /// <summary>
    /// Params: <c>cardName</c>, optional <c>locale</c>.
    /// Returns a serialised <see cref="RenderNode"/> tree.
    /// </summary>
    public object Handle(JsonElement? p)
    {
        var cardName = p?.GetProperty("cardName").GetString()
            ?? throw new ArgumentException("RenderCardText: 'cardName' required.");
        string? locale = p?.TryGetProperty("locale", out var lel) == true
            ? lel.GetString() : null;

        var state = sidecar.State;
        if (!state.Cards.TryGetValue(cardName, out var card))
            return new { error = $"Card '{cardName}' not found." };

        if (card.PrimaryEffectNode is null)
            return new { error = "Primary effect has parse errors; cannot render." };

        // Build a minimal keyword definition lookup from ProjectState.
        var keywords = BuildKeywordDefs(state);
        var renderer = new TextRenderer(keywords);

        // Resolve locale strings if provided.
        IReadOnlyDictionary<string, string>? localeStrings = null;
        if (locale is not null &&
            state.Localization.Strings.TryGetValue(locale, out var table))
            localeStrings = table;

        // Definition-time render: pass null bindings so ParameterRefs render as labels.
        var renderNode = renderer.RenderBlock(card.PrimaryEffectNode, localeStrings, null);
        var flatText   = TextRenderer.FlattenToText(renderNode);

        return new { renderNode, flatText };
    }

    // -----------------------------------------------------------------------
    //  Build keyword definitions from ProjectState
    // -----------------------------------------------------------------------

    private static IReadOnlyDictionary<string, KeywordDefinition> BuildKeywordDefs(
        ProjectState state)
    {
        var dict = new Dictionary<string, KeywordDefinition>(StringComparer.Ordinal);

        // Built-ins first
        foreach (var builtin in BuiltInKeywords.All)
            dict[builtin.Name] = builtin;

        // Game-creator keywords
        foreach (var (name, kw) in state.Keywords)
        {
            if (kw.BodyNode is null) continue;
            dict[name] = new KeywordDefinition(
                Name:          name,
                Parameters:    kw.Parameters.ToArray(),
                ReturnType:    TypeName.Atom, // return type not needed for rendering
                Description:   "",
                Body:          kw.BodyNode,  // KeywordNode? — the composite body
                TextTemplate:  kw.TextTemplate);
        }

        return dict;
    }
}
