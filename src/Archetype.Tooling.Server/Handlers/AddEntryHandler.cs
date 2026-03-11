using System.Text.Json;

namespace Archetype.Tooling.Server.Handlers;

/// <summary>
/// Handles <c>AddEntry</c> — adds a new empty entry of the given kind.
/// </summary>
public sealed class AddEntryHandler(SidecarState sidecar)
{
    /// <summary>
    /// Params: <c>entryKind</c>, <c>entryName</c>.
    /// </summary>
    public object Handle(JsonElement? p)
    {
        var kind = p?.GetProperty("entryKind").GetString()
            ?? throw new ArgumentException("AddEntry: 'entryKind' required.");
        var name = p?.GetProperty("entryName").GetString()
            ?? throw new ArgumentException("AddEntry: 'entryName' required.");

        var state = sidecar.State;

        switch (kind)
        {
            case "keyword":
                if (!state.Keywords.ContainsKey(name))
                    state.Keywords[name] = new KeywordEntry { Name = name };
                break;

            case "card":
                if (!state.Cards.ContainsKey(name))
                    state.Cards[name] = new CardEntry { Name = name };
                break;

            case "zone":
                if (!state.Zones.ContainsKey(name))
                    state.Zones[name] = new ZoneEntry { Name = name };
                break;

            case "player":
                if (!state.Players.ContainsKey(name))
                    state.Players[name] = new PlayerEntry { Name = name };
                break;

            case "cardSet":
                if (!state.CardSets.ContainsKey(name))
                    state.CardSets[name] = new CardSetEntry { Name = name };
                break;

            case "phase":
                state.Phases.Add(new PhaseEntry { Name = name });
                break;

            case "stateBasedRule":
                state.StateBasedRules.Add(new StateBasedRuleEntry { Name = name });
                break;

            default:
                throw new ArgumentException($"AddEntry: unknown entryKind '{kind}'.");
        }

        return MutationHelpers.RevalidateAndBuildResponse(state, [name]);
    }
}
