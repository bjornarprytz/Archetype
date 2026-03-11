using System.Text.Json;

namespace Archetype.Tooling.Server.Handlers;

/// <summary>
/// Handles <c>UpdateActivationCondition</c> — parses a new activation condition
/// DSL for a card or named effect.
/// </summary>
public sealed class UpdateActivationConditionHandler(SidecarState sidecar)
{
    /// <summary>
    /// Params: <c>cardName</c>, <c>dsl</c>,
    /// optional <c>effectName</c> (for ability-level conditions).
    /// </summary>
    public object Handle(JsonElement? p)
    {
        var cardName = p?.GetProperty("cardName").GetString()
            ?? throw new ArgumentException("UpdateActivationCondition: 'cardName' required.");
        var dsl = p?.GetProperty("dsl").GetString()
            ?? throw new ArgumentException("UpdateActivationCondition: 'dsl' required.");
        string? effectName = p?.TryGetProperty("effectName", out var en) == true
            ? en.GetString() : null;

        var state = sidecar.State;
        if (!state.Cards.TryGetValue(cardName, out var card))
            throw new ArgumentException($"UpdateActivationCondition: card '{cardName}' not found.");

        card.Diagnostics = [];
        var result = DslParser.Parse(dsl);

        if (effectName is null)
        {
            card.ActivationConditionDsl  = dsl;
            card.ActivationConditionNode = result.IsSuccess ? result.Node : null;
        }
        else
        {
            var eff = card.AdditionalEffects.FirstOrDefault(e => e.Name == effectName);
            if (eff is null)
                throw new ArgumentException(
                    $"UpdateActivationCondition: effect '{effectName}' not found on '{cardName}'.");
            eff.ActivationConditionDsl  = dsl;
            eff.ActivationConditionNode = result.IsSuccess ? result.Node : null;
        }

        if (!result.IsSuccess)
            card.Diagnostics.Add(new ProjectDiagnostic(
                "card", cardName, "error",
                result.ErrorMessage ?? "Parse error.",
                result.ErrorOffset.HasValue
                    ? new DslRange(result.ErrorOffset.Value, dsl.Length)
                    : null));

        return MutationHelpers.RevalidateAndBuildResponse(state, [cardName]);
    }
}
