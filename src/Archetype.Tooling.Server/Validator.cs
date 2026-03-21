using Archetype.Core;

namespace Archetype.Tooling.Server;

// ---------------------------------------------------------------------------
//  Validator — full cross-entry validation pass (D27, D28, D31)
// ---------------------------------------------------------------------------

/// <summary>
/// Runs the full validation pass over a <see cref="ProjectState"/> and
/// populates <see cref="ProjectState.Diagnostics"/>.
/// <para>
/// Validation checks:
/// <list type="bullet">
/// <item>Duplicate entry names across all definition kinds.</item>
/// <item>Unresolved keyword references in parsed <see cref="KeywordNode"/> trees.</item>
/// <item>Missing-translation warnings per locale (D31).</item>
/// </list>
/// </para>
/// <para>
/// The initial implementation performs a full project re-validation on every
/// call.  D28 notes that incremental validation is deferred.
/// </para>
/// </summary>
public static class Validator
{
    /// <summary>
    /// Runs the full validation pass.  All per-entry and cross-entry
    /// diagnostics are written to <see cref="ProjectState.Diagnostics"/>.
    /// </summary>
    public static void Validate(ProjectState state)
    {
        state.Diagnostics.Clear();

        // Collect per-entry diagnostics first.
        CollectEntryDiagnostics(state);

        // Cross-entry: duplicate names (D28 — same name in different entry kinds is fine;
        // duplicates within the same kind are already prevented by Dictionary keying).
        // We do check for duplicate keyword names between game-creator keywords and built-ins.
        CheckBuiltInNameConflicts(state);

        // Cross-entry: unresolved keyword references in all parsed trees.
        CheckUnresolvedReferences(state);

        // Cross-entry: missing translations (D31 warnings).
        CheckMissingTranslations(state);
    }

    // -----------------------------------------------------------------------
    //  Per-entry diagnostic collection
    // -----------------------------------------------------------------------

    private static void CollectEntryDiagnostics(ProjectState state)
    {
        foreach (var kw in state.Keywords.Values)
            state.Diagnostics.AddRange(kw.Diagnostics);

        foreach (var card in state.Cards.Values)
        {
            state.Diagnostics.AddRange(card.Diagnostics);
            // StaticEffectEntry has its own Diagnostics list (e.g. lifetime parse errors);
            // collect them so they appear in state.Diagnostics and are reflected in
            // GlobalErrorCount returned to the renderer.
            foreach (var se in card.StaticEffects)
                state.Diagnostics.AddRange(se.Diagnostics);
        }

        foreach (var zone in state.Zones.Values)
            state.Diagnostics.AddRange(zone.Diagnostics);

        foreach (var player in state.Players.Values)
            state.Diagnostics.AddRange(player.Diagnostics);

        foreach (var cs in state.CardSets.Values)
            state.Diagnostics.AddRange(cs.Diagnostics);

        foreach (var phase in state.Phases)
            state.Diagnostics.AddRange(phase.Diagnostics);

        foreach (var rules in state.ActionRules.Values)
            foreach (var r in rules)
                state.Diagnostics.AddRange(r.Diagnostics);

        foreach (var sbr in state.StateBasedRules)
            state.Diagnostics.AddRange(sbr.Diagnostics);

        if (state.InitManifest is not null)
            state.Diagnostics.AddRange(state.InitManifest.Diagnostics);
    }

    // -----------------------------------------------------------------------
    //  Built-in name conflicts
    // -----------------------------------------------------------------------

    private static void CheckBuiltInNameConflicts(ProjectState state)
    {
        var builtIns = new HashSet<string>(
            BuiltInKeywords.All.Select(k => k.Name),
            StringComparer.Ordinal);

        foreach (var (name, kw) in state.Keywords)
        {
            if (builtIns.Contains(name))
                state.Diagnostics.Add(new ProjectDiagnostic(
                    "keyword", name, "error",
                    $"Keyword name '{name}' conflicts with a built-in keyword. " +
                    "Game-creator keywords may not shadow built-in names (D2)."));

            // D27: ReturnType is mandatory on every keyword entry.
            if (kw.ReturnType is null)
                state.Diagnostics.Add(new ProjectDiagnostic(
                    "keyword", name, "error",
                    $"Keyword '{name}' is missing a return type declaration. " +
                    "Add a 'returnType' field (e.g. \"returnType\": \"Atom\") (D27)."));
        }
    }

    // -----------------------------------------------------------------------
    //  Unresolved keyword references
    // -----------------------------------------------------------------------

    private static void CheckUnresolvedReferences(ProjectState state)
    {
        var known = new HashSet<string>(state.AllKeywordNames(), StringComparer.Ordinal);

        // Collect all Invocation nodes across the whole project.
        foreach (var (name, kw) in state.Keywords)
        {
            if (kw.BodyNode is not null)
                CheckNodeRefs(kw.BodyNode, "keyword", name, known, state);
        }

        foreach (var (name, card) in state.Cards)
        {
            if (card.PrimaryEffectNode is not null)
                CheckBlockRefs(card.PrimaryEffectNode, "card", name, known, state);
            if (card.ActivationConditionNode is not null)
                CheckNodeRefs(card.ActivationConditionNode, "card", name, known, state);
            foreach (var eff in card.AdditionalEffects)
                if (eff.BodyNode is not null)
                    CheckBlockRefs(eff.BodyNode, "card", name, known, state);
        }

        foreach (var phase in state.Phases)
        {
            if (phase.InitNode is not null)    CheckBlockRefs(phase.InitNode,    "phase", phase.Name, known, state);
            if (phase.CleanupNode is not null) CheckBlockRefs(phase.CleanupNode, "phase", phase.Name, known, state);
        }

        foreach (var sbr in state.StateBasedRules)
        {
            if (sbr.ConditionNode is not null) CheckNodeRefs(sbr.ConditionNode, "stateBasedRule", sbr.Name, known, state);
            if (sbr.BodyNode is not null)      CheckBlockRefs(sbr.BodyNode,     "stateBasedRule", sbr.Name, known, state);
        }
    }

    private static void CheckBlockRefs(
        EffectBlockDef block, string kind, string name,
        HashSet<string> known, ProjectState state)
    {
        foreach (var step in block.Steps)
            CheckNodeRefs(new Invocation(step.KeywordName, step.ArgNodes), kind, name, known, state);
    }

    private static void CheckNodeRefs(
        KeywordNode node, string kind, string entryName,
        HashSet<string> known, ProjectState state)
    {
        if (node is Invocation inv)
        {
            if (!known.Contains(inv.KeywordName))
                state.Diagnostics.Add(new ProjectDiagnostic(
                    kind, entryName, "error",
                    $"Unresolved keyword reference '{inv.KeywordName}'."));

            foreach (var arg in inv.Args)
                CheckNodeRefs(arg, kind, entryName, known, state);
        }
    }

    // -----------------------------------------------------------------------
    //  Missing translations (D31 warnings)
    // -----------------------------------------------------------------------

    private static void CheckMissingTranslations(ProjectState state)
    {
        var allLocales = state.Localization.Strings.Keys
            .Where(l => !string.Equals(l, state.Localization.SourceLanguage,
                StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var locale in allLocales)
        {
            var missing = state.Localization.MissingKeysForLocale(locale);
            foreach (var key in missing)
            {
                // Determine which entry this key belongs to.
                string entryKind = "keyword";
                string entryName = key;

                state.Diagnostics.Add(new ProjectDiagnostic(
                    entryKind, entryName, "warning",
                    $"Missing translation for locale '{locale}'."));
            }
        }
    }
}
