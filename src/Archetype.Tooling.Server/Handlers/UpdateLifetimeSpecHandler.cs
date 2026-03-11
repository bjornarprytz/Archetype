using System.Text.Json;

namespace Archetype.Tooling.Server.Handlers;

/// <summary>
/// Handles <c>UpdateLifetimeSpec</c> — stores a lifetime DSL string on a
/// static effect entry.  The sidecar does not yet parse lifetime specs into
/// <see cref="Archetype.Core.LifetimeSpec"/>; it stores the raw string for
/// round-tripping and includes it in export generation.
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

        card.StaticEffects[effectIndex].LifetimeDsl = dsl;

        return MutationHelpers.RevalidateAndBuildResponse(state, [cardName]);
    }
}
