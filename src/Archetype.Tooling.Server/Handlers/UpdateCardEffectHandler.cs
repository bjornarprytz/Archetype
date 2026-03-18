using System.Text.Json;

namespace Archetype.Tooling.Server.Handlers;

/// <summary>
/// Handles <c>UpdateCardEffect</c> — parses new DSL for a card's primary or
/// named effect block and returns scoped diagnostics.
/// </summary>
public sealed class UpdateCardEffectHandler(SidecarState sidecar)
{
    /// <summary>
    /// Params: <c>cardName</c>, <c>effectName</c> (omit or <c>null</c> for primary),
    /// <c>dsl</c>.
    /// </summary>
    public object Handle(JsonElement? p)
    {
        var cardName   = p?.GetProperty("cardName").GetString()
            ?? throw new ArgumentException("UpdateCardEffect: 'cardName' required.");
        var dsl        = p?.GetProperty("dsl").GetString()
            ?? throw new ArgumentException("UpdateCardEffect: 'dsl' required.");
        string? effectName = p?.TryGetProperty("effectName", out var en) == true
            ? en.GetString() : null;

        var state = sidecar.State;
        if (!state.Cards.TryGetValue(cardName, out var card))
        {
            card = new CardEntry { Name = cardName };
            state.Cards[cardName] = card;
        }

        card.Diagnostics = [];

        var r = DslParser.ParseBlock(dsl);

        if (effectName is null)
        {
            // Primary effect
            card.PrimaryEffectDsl  = dsl;
            card.PrimaryEffectNode = r.IsSuccess ? r.Block : null;
        }
        else
        {
            // Named effect
            var eff = card.AdditionalEffects.FirstOrDefault(e => e.Name == effectName)
                ?? new NamedEffectEntry { Name = effectName };
            if (!card.AdditionalEffects.Contains(eff))
                card.AdditionalEffects.Add(eff);
            eff.BodyDsl  = dsl;
            eff.BodyNode = r.IsSuccess ? r.Block : null;
        }

        foreach (var d in r.Diagnostics)
            card.Diagnostics.Add(new ProjectDiagnostic(
                "card", cardName, "error", d.Message, new DslRange(d.Start, d.End)));

        return MutationHelpers.RevalidateAndBuildResponse(state, [cardName]);
    }
}
