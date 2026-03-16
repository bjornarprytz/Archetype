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

        switch (kind.ToLowerInvariant())
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
    /// Also rewrites all <c>*Dsl</c> source strings so the project file saved
    /// to disk does not contain stale references (D27 — DSL text is canonical).
    /// </summary>
    private static void RewriteKeywordRefs(
        ProjectState state, string oldName, string newName)
    {
        // Rewrite node trees (used for validation and export in the current session).
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

        // D27: DSL text is the canonical form stored in the project file.
        // Rewrite all *Dsl source strings so a save→reload round-trip is clean.
        foreach (var kw in state.Keywords.Values)
            kw.BodyDsl = RewriteDsl(kw.BodyDsl, oldName, newName);

        foreach (var card in state.Cards.Values)
        {
            card.PrimaryEffectDsl = RewriteDsl(card.PrimaryEffectDsl, oldName, newName);
            if (card.ActivationConditionDsl is not null)
                card.ActivationConditionDsl =
                    RewriteDsl(card.ActivationConditionDsl, oldName, newName);
            foreach (var eff in card.AdditionalEffects)
            {
                eff.BodyDsl = RewriteDsl(eff.BodyDsl, oldName, newName);
                if (eff.ActivationConditionDsl is not null)
                    eff.ActivationConditionDsl =
                        RewriteDsl(eff.ActivationConditionDsl, oldName, newName);
                foreach (var cost in eff.Costs)
                    cost.BodyDsl = RewriteDsl(cost.BodyDsl, oldName, newName);
            }
            foreach (var cost in card.Costs)
                cost.BodyDsl = RewriteDsl(cost.BodyDsl, oldName, newName);
            foreach (var se in card.StaticEffects)
            {
                se.ContributionDsl = RewriteDsl(se.ContributionDsl, oldName, newName);
                if (se.TriggerConditionDsl is not null)
                    se.TriggerConditionDsl =
                        RewriteDsl(se.TriggerConditionDsl, oldName, newName);
                if (se.TriggerBodyDsl is not null)
                    se.TriggerBodyDsl = RewriteDsl(se.TriggerBodyDsl, oldName, newName);
            }
        }

        // D27 requires all DSL-carrying entry types to be rewritten (BLOCKER 2).
        // Phase init/cleanup blocks may invoke game-creator keywords.
        foreach (var phase in state.Phases)
        {
            if (phase.InitDsl is not null)
                phase.InitDsl = RewriteDsl(phase.InitDsl, oldName, newName);
            if (phase.CleanupDsl is not null)
                phase.CleanupDsl = RewriteDsl(phase.CleanupDsl, oldName, newName);
        }

        // Action rule before/after hooks may invoke game-creator keywords.
        foreach (var rules in state.ActionRules.Values)
            foreach (var rule in rules)
            {
                if (rule.BeforeDsl is not null)
                    rule.BeforeDsl = RewriteDsl(rule.BeforeDsl, oldName, newName);
                if (rule.AfterDsl is not null)
                    rule.AfterDsl = RewriteDsl(rule.AfterDsl, oldName, newName);
            }

        // State-based rule conditions and bodies may invoke game-creator keywords.
        foreach (var sbr in state.StateBasedRules)
        {
            sbr.ConditionDsl = RewriteDsl(sbr.ConditionDsl, oldName, newName);
            sbr.BodyDsl      = RewriteDsl(sbr.BodyDsl, oldName, newName);
        }
    }

    /// <summary>
    /// Replaces all whole-token occurrences of <paramref name="oldName"/> with
    /// <paramref name="newName"/> in a DSL source string.
    /// Uses a word-boundary approach: a token boundary is any character that is
    /// not a letter, digit, underscore, or hyphen.
    /// </summary>
    private static string RewriteDsl(string dsl, string oldName, string newName)
    {
        if (string.IsNullOrEmpty(dsl) || !dsl.Contains(oldName, StringComparison.Ordinal))
            return dsl;

        // Build result by scanning for the old name and checking token boundaries.
        var sb = new System.Text.StringBuilder(dsl.Length);
        int pos = 0;
        while (pos < dsl.Length)
        {
            int idx = dsl.IndexOf(oldName, pos, StringComparison.Ordinal);
            if (idx == -1)
            {
                sb.Append(dsl, pos, dsl.Length - pos);
                break;
            }

            // Check that the match is not part of a longer identifier.
            bool startOk = idx == 0 || !IsIdentChar(dsl[idx - 1]);
            int end = idx + oldName.Length;
            bool endOk = end >= dsl.Length || !IsIdentChar(dsl[end]);

            sb.Append(dsl, pos, idx - pos);
            sb.Append(startOk && endOk ? newName : oldName);
            pos = end;
        }
        return sb.ToString();
    }

    private static bool IsIdentChar(char c) =>
        char.IsLetterOrDigit(c) || c == '_' || c == '-';

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
