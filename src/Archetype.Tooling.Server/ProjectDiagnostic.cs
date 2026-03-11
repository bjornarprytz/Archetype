namespace Archetype.Tooling.Server;

// ---------------------------------------------------------------------------
//  ProjectDiagnostic — per-entry authoring error/warning (D27, D28)
// ---------------------------------------------------------------------------

/// <summary>
/// A single error or warning produced during project validation or DSL parsing.
/// Diagnostics are scoped to the entry that produced them.
/// </summary>
/// <param name="EntryKind">
/// The kind of definition entry — one of <c>"keyword"</c>, <c>"card"</c>,
/// <c>"zone"</c>, <c>"player"</c>, <c>"phase"</c>, <c>"actionRule"</c>,
/// <c>"stateBasedRule"</c>, <c>"initManifest"</c>, <c>"project"</c>.
/// </param>
/// <param name="EntryName">
/// Name of the specific entry within its kind (e.g. <c>"take-damage"</c>,
/// <c>"goblin"</c>).  May be empty for project-level diagnostics.
/// </param>
/// <param name="Severity">
/// <c>"error"</c> blocks export; <c>"warning"</c> does not (D31).
/// </param>
/// <param name="Message">Human-readable description of the issue.</param>
/// <param name="DslRange">
/// Optional character-offset range into the entry's DSL source string.
/// Present when the diagnostic arises from a DSL parse or type-check error.
/// </param>
public sealed record ProjectDiagnostic(
    string EntryKind,
    string EntryName,
    string Severity,
    string Message,
    DslRange? DslRange = null);

/// <summary>
/// Character-offset range into a DSL source string (zero-based, inclusive start,
/// exclusive end).  The renderer translates this to a Monaco <c>IRange</c>
/// via <c>model.getPositionAt()</c>.
/// </summary>
public sealed record DslRange(int Start, int End);
