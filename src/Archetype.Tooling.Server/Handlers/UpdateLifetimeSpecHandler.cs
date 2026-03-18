using System.Text.Json;

namespace Archetype.Tooling.Server.Handlers;

/// <summary>
/// Handles <c>UpdateLifetimeSpec</c> — stores a lifetime DSL string on a
/// static effect entry and immediately parses it into a
/// <see cref="Archetype.Core.LifetimeSpec"/> so that in-session exports
/// reflect the authored spec without requiring a save/reload cycle (D27).
/// </summary>
public sealed class UpdateLifetimeSpecHandler(SidecarState sidecar)
{
    /// <summary>
    /// Params: <c>cardName</c>, <c>effectIndex</c> (int), <c>dsl</c>.
    /// </summary>
    public object Handle(JsonElement? p)
    {
        var cardName   = p?.GetProperty("cardName").GetString()
            ?? throw new ArgumentException("UpdateLifetimeSpec: 'cardName' required.");
        var effectIndex = p?.GetProperty("effectIndex").GetInt32()
            ?? throw new ArgumentException("UpdateLifetimeSpec: 'effectIndex' required.");
        var dsl = p?.GetProperty("dsl").GetString()
            ?? throw new ArgumentException("UpdateLifetimeSpec: 'dsl' required.");

        var state = sidecar.State;
        if (!state.Cards.TryGetValue(cardName, out var card))
            throw new ArgumentException($"UpdateLifetimeSpec: card '{cardName}' not found.");

        if (effectIndex < 0 || effectIndex >= card.StaticEffects.Count)
            throw new ArgumentOutOfRangeException(nameof(effectIndex),
                $"Static effect index {effectIndex} out of range.");

        var effect = card.StaticEffects[effectIndex];
        effect.LifetimeDsl  = dsl;

        // Parse immediately so the exporter sees the updated LifetimeNode in the
        // same session — without this the exporter falls back to LifetimeSpec.Permanent
        // for any edit that hasn't been saved and reloaded (BLOCKER 1).
        effect.LifetimeNode = LifetimeDsl.Parse(dsl);

        // A null result means the DSL was non-empty but unparseable; the validator
        // will record the diagnostic via the card's entry diagnostics on next pass.
        if (effect.LifetimeNode is null && !string.IsNullOrWhiteSpace(dsl))
        {
            effect.Diagnostics.Add(new ProjectDiagnostic(
                "card", cardName, "error",
                $"[staticEffect.lifetime] Could not parse lifetime DSL: '{dsl}'."));
        }

        return MutationHelpers.RevalidateAndBuildResponse(state, [cardName]);
    }
}
