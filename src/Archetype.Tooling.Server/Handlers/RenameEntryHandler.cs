using System.Text.Json;
using Archetype.Core;

namespace Archetype.Tooling.Server.Handlers;

/// <summary>
/// Handles <c>RenameEntry</c> — renames an entry and rewrites all call-site
/// references in parsed <see cref="KeywordNode"/> trees.
/// </summary>
public sealed class RenameEntryHandler(SidecarState sidecar)
{
    /// <summary>
    /// Params: <c>entryKind</c>, <c>oldName</c>, <c>newName</c>.
    /// </summary>
    public object Handle(JsonElement? p)
    {
        var kind    = p?.GetProperty("entryKind").GetString()
            ?? throw new ArgumentException("RenameEntry: 'entryKind' required.");
        var oldName = p?.GetProperty("oldName").GetString()
            ?? throw new ArgumentException("RenameEntry: 'oldName' required.");
        var newName = p?.GetProperty("newName").GetString()
            ?? throw new ArgumentException("RenameEntry: 'newName' required.");

        if (string.Equals(oldName, newName, StringComparison.Ordinal))
            return MutationHelpers.RevalidateAndBuildResponse(sidecar.State, [oldName]);

        var state = sidecar.State;

        switch (kind)
        {
            case "keyword":
                if (state.Keywords.TryGetValue(oldName, out var kw))
                {
                    state.Keywords.Remove(oldName);
                    kw.Name = newName;
                    state.Keywords[newName] = kw;
                    // Rewrite all call sites that reference oldName.
                    RewriteKeywordRefs(state, oldName, newName);
                }
                break;

            case "card":
                if (state.Cards.TryGetValue(oldName, out var card))
                {
                    state.Cards.Remove(oldName);
                    card.Name = newName;
                    state.Cards[newName] = card;
                }
                break;

            case "zone":
                if (state.Zones.TryGetValue(oldName, out var zone))
                {
                    state.Zones.Remove(oldName);
                    zone.Name = newName;
                    state.Zones[newName] = zone;
                }
                break;

            case "player":
                if (state.Players.TryGetValue(oldName, out var player))
                {
                    state.Players.Remove(oldName);
                    player.Name = newName;
                    state.Players[newName] = player;
                }
                break;

            case "cardSet":
                if (state.CardSets.TryGetValue(oldName, out var cs))
                {
                    state.CardSets.Remove(oldName);
                    cs.Name = newName;
                    state.CardSets[newName] = cs;
                }
                break;

            default:
                throw new ArgumentException($"RenameEntry: unsupported entryKind '{kind}'.");
        }

        // Affected = callers of the old name (they now reference new name).
        state.UsedBy.TryGetValue(oldName, out var callers);
        var affected = new List<string> { newName };
        if (callers is not null) affected.AddRange(callers);

        return MutationHelpers.RevalidateAndBuildResponse(state, affected);
    }

    // -----------------------------------------------------------------------
    //  Keyword reference rewriting
    // -----------------------------------------------------------------------

    /// <summary>
    /// Rewrites every <see cref="Invocation"/> node in all keyword bodies and
    /// card effect trees that references <paramref name="oldName"/> to
    /// <paramref name="newName"/>.
    /// The DSL source strings are NOT rewritten here (they are stale until the
    /// game creator next edits the field); the node trees are updated so
    /// subsequent validation and export use the new name.
    /// </summary>
    private static void RewriteKeywordRefs(
        ProjectState state, string oldName, string newName)
    {
        foreach (var kw in state.Keywords.Values)
            if (kw.BodyNode is not null)
                kw.BodyNode = RenameInNode(kw.BodyNode, oldName, newName);

        foreach (var card in state.Cards.Values)
        {
            if (card.PrimaryEffectNode is not null)
                card.PrimaryEffectNode = RenameInBlock(card.PrimaryEffectNode, oldName, newName);
            foreach (var eff in card.AdditionalEffects)
                if (eff.BodyNode is not null)
                    eff.BodyNode = RenameInBlock(eff.BodyNode, oldName, newName);
        }
    }

    private static EffectBlockDef RenameInBlock(
        EffectBlockDef block, string oldName, string newName)
    {
        var steps = block.Steps.Select(s =>
        {
            // Rename at the top-level step keyword name if it matches.
            var renamedName = s.KeywordName == oldName ? newName : s.KeywordName;
            // Rename within argument trees.
            var renamedArgs = s.ArgNodes.Select(a => RenameInNode(a, oldName, newName)).ToArray();
            return new EffectBlockStep(renamedName, renamedArgs, s.BindTo);
        }).ToList();
        return new EffectBlockDef(steps);
    }

    private static KeywordNode RenameInNode(
        KeywordNode node, string oldName, string newName) =>
        node switch
        {
            Invocation inv when inv.KeywordName == oldName =>
                new Invocation(newName, inv.Args.Select(a => RenameInNode(a, oldName, newName)).ToArray()),
            Invocation inv =>
                new Invocation(inv.KeywordName, inv.Args.Select(a => RenameInNode(a, oldName, newName)).ToArray()),
            _ => node,
        };
}
