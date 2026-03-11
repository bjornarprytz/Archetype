using System.Text.Json;

namespace Archetype.Tooling.Server.Handlers;

/// <summary>
/// Handles <c>RemoveEntry</c> — removes an entry and returns orphan diagnostics
/// for any call sites that referenced it.
/// </summary>
public sealed class RemoveEntryHandler(SidecarState sidecar)
{
    /// <summary>
    /// Params: <c>entryKind</c>, <c>entryName</c>.
    /// </summary>
    public object Handle(JsonElement? p)
    {
        var kind = p?.GetProperty("entryKind").GetString()
            ?? throw new ArgumentException("RemoveEntry: 'entryKind' required.");
        var name = p?.GetProperty("entryName").GetString()
            ?? throw new ArgumentException("RemoveEntry: 'entryName' required.");

        var state = sidecar.State;

        // Collect callers before removal (they will get unresolved-ref diagnostics).
        state.UsedBy.TryGetValue(name, out var callers);
        var affected = callers?.ToList() ?? [];

        switch (kind)
        {
            case "keyword":   state.Keywords.Remove(name);                                           break;
            case "card":      state.Cards.Remove(name);                                              break;
            case "zone":      state.Zones.Remove(name);                                              break;
            case "player":    state.Players.Remove(name);                                            break;
            case "cardSet":   state.CardSets.Remove(name);                                           break;
            case "phase":     state.Phases.RemoveAll(ph => ph.Name == name);                         break;
            case "stateBasedRule": state.StateBasedRules.RemoveAll(r => r.Name == name);             break;
            default: throw new ArgumentException($"RemoveEntry: unknown entryKind '{kind}'.");
        }

        return MutationHelpers.RevalidateAndBuildResponse(state, affected);
    }
}
