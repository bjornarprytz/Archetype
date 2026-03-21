using Archetype.Core;

namespace Archetype.Tooling.Server;

// ---------------------------------------------------------------------------
//  ProjectState — in-memory authoring state for one .archetype project (D27)
// ---------------------------------------------------------------------------

/// <summary>
/// Mutable in-memory state of an open project.  This is the sidecar's single
/// source of truth for the lifetime of a session (D27).
/// <para>
/// Unlike <see cref="GameDefinition"/>, <c>ProjectState</c> is lenient — it
/// permits partially-parsed or structurally invalid entries.  Entries with
/// DSL errors have their parsed node set to <c>null</c> and a
/// <see cref="ProjectDiagnostic"/> recorded.
/// </para>
/// </summary>
public sealed class ProjectState
{
    // -----------------------------------------------------------------------
    //  Identity
    // -----------------------------------------------------------------------

    /// <summary>Game definition ID (mirrors <see cref="GameDefinition.Id"/>).</summary>
    public string? Id { get; set; }

    // -----------------------------------------------------------------------
    //  Definition entries
    // -----------------------------------------------------------------------

    /// <summary>All keyword definitions, keyed by name.</summary>
    public Dictionary<string, KeywordEntry> Keywords { get; set; } = new(StringComparer.Ordinal);

    /// <summary>All card definitions, keyed by name.</summary>
    public Dictionary<string, CardEntry> Cards { get; set; } = new(StringComparer.Ordinal);

    /// <summary>All zone definitions, keyed by name.</summary>
    public Dictionary<string, ZoneEntry> Zones { get; set; } = new(StringComparer.Ordinal);

    /// <summary>All player definitions, keyed by name.</summary>
    public Dictionary<string, PlayerEntry> Players { get; set; } = new(StringComparer.Ordinal);

    /// <summary>All card sets, keyed by name.</summary>
    public Dictionary<string, CardSetEntry> CardSets { get; set; } = new(StringComparer.Ordinal);

    /// <summary>Phase definitions in turn order.</summary>
    public List<PhaseEntry> Phases { get; set; } = [];

    /// <summary>
    /// Action rule definitions, keyed by action type name.
    /// Each action type may have multiple rules.
    /// </summary>
    public Dictionary<string, List<ActionRuleEntry>> ActionRules { get; set; } =
        new(StringComparer.Ordinal);

    /// <summary>State-based rules in evaluation order.</summary>
    public List<StateBasedRuleEntry> StateBasedRules { get; set; } = [];

    /// <summary>Trigger resolution order for this game.</summary>
    public TriggerResolutionOrder TriggerResolutionOrder { get; set; } =
        TriggerResolutionOrder.OldestFirst;

    /// <summary>Default game setup manifest (required for export; may be null during authoring).</summary>
    public InitManifestEntry? InitManifest { get; set; }

    /// <summary>Zone definition names from which cards may be played (null = no zone filter).</summary>
    public List<string> PlayableZoneNames { get; set; } = [];

    // -----------------------------------------------------------------------
    //  Localization
    // -----------------------------------------------------------------------

    /// <summary>All localisation strings and source language configuration.</summary>
    public LocalizationState Localization { get; set; } = new();

    // -----------------------------------------------------------------------
    //  Diagnostics (union of all per-entry diagnostics)
    // -----------------------------------------------------------------------

    /// <summary>
    /// All diagnostics accumulated during the last validation pass.
    /// This is the union of every per-entry <c>Diagnostics</c> list plus
    /// cross-entry diagnostics (unresolved references, duplicate names, etc.).
    /// </summary>
    public List<ProjectDiagnostic> Diagnostics { get; set; } = [];

    /// <summary>Convenience: count of error-severity diagnostics.</summary>
    public int ErrorCount =>
        Diagnostics.Count(d => d.Severity == "error");

    /// <summary>Convenience: count of warning-severity diagnostics.</summary>
    public int WarningCount =>
        Diagnostics.Count(d => d.Severity == "warning");

    // -----------------------------------------------------------------------
    //  Tooling editor state (round-tripped verbatim per D27)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Opaque editor state from the <c>tooling</c> section of the project file.
    /// The sidecar does not interpret this; it stores and returns it verbatim.
    /// </summary>
    public System.Text.Json.JsonElement? EditorState { get; set; }

    // -----------------------------------------------------------------------
    //  Reference graph (maintained by Validator; rebuilt on each mutation)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Reverse reference graph: <c>usedBy[entryName] = {names that call entryName}</c>.
    /// Built by <see cref="ReferenceGraph"/> from all parsed keyword trees.
    /// Used for impact propagation (D28) and symbol navigation.
    /// </summary>
    public Dictionary<string, HashSet<string>> UsedBy { get; set; } =
        new(StringComparer.Ordinal);

    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns all entry names across all entry kinds.
    /// Used for duplicate-name detection.
    /// </summary>
    public IEnumerable<(string Kind, string Name)> AllEntryNames()
    {
        foreach (var k in Keywords.Keys)    yield return ("keyword",       k);
        foreach (var k in Cards.Keys)       yield return ("card",          k);
        foreach (var k in Zones.Keys)       yield return ("zone",          k);
        foreach (var k in Players.Keys)     yield return ("player",        k);
        foreach (var k in CardSets.Keys)    yield return ("cardSet",       k);
        foreach (var r in StateBasedRules)  yield return ("stateBasedRule", r.Name);
    }

    /// <summary>
    /// Returns all currently known keyword names (built-in + game-creator).
    /// Used for completions and reference resolution.
    /// </summary>
    public IEnumerable<string> AllKeywordNames() =>
        BuiltInKeywords.All.Select(k => k.Name).Concat(Keywords.Keys);
}
