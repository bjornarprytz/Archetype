using System.Text.Json;

namespace Archetype.Tooling.Server.Handlers;

/// <summary>
/// Handles <c>UpdateCostBody</c> — parses DSL for a card or ability cost entry.
/// </summary>
public sealed class UpdateCostBodyHandler(SidecarState sidecar)
{
    /// <summary>
    /// Params: <c>cardName</c>, <c>costIndex</c> (int), <c>dsl</c>,
    /// optional <c>effectName</c> (for ability-level costs).
    /// </summary>
    public object Handle(JsonElement? p)
    {
        var cardName  = p?.GetProperty("cardName").GetString()
            ?? throw new ArgumentException("UpdateCostBody: 'cardName' required.");
        var costIndex = p?.GetProperty("costIndex").GetInt32()
            ?? throw new ArgumentException("UpdateCostBody: 'costIndex' required.");
        var dsl       = p?.GetProperty("dsl").GetString()
            ?? throw new ArgumentException("UpdateCostBody: 'dsl' required.");
        string? effectName = p?.TryGetProperty("effectName", out var en) == true
            ? en.GetString() : null;

        var state = sidecar.State;
        if (!state.Cards.TryGetValue(cardName, out var card))
            throw new ArgumentException($"UpdateCostBody: card '{cardName}' not found.");

        List<CostEntry> costs = effectName is null
            ? card.Costs
            : card.AdditionalEffects.FirstOrDefault(e => e.Name == effectName)?.Costs
              ?? throw new ArgumentException($"UpdateCostBody: effect '{effectName}' not found.");

        // Grow the list if needed.
        while (costs.Count <= costIndex)
            costs.Add(new CostEntry());

        var r = DslParser.ParseBlock(dsl);
        costs[costIndex].BodyDsl  = dsl;
        costs[costIndex].BodyNode = r.IsSuccess ? r.Block : null;

        card.Diagnostics = [];
        foreach (var d in r.Diagnostics)
            card.Diagnostics.Add(new ProjectDiagnostic(
                "card", cardName, "error", d.Message, new DslRange(d.Start, d.End)));

        return MutationHelpers.RevalidateAndBuildResponse(state, [cardName]);
    }
}
