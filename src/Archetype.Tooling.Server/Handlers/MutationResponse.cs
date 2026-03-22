namespace Archetype.Tooling.Server.Handlers;

// ---------------------------------------------------------------------------
//  MutationResponse — standard return shape for all mutation methods (D28)
// ---------------------------------------------------------------------------

/// <summary>
/// Standard response for all mutation methods per D28.
/// The renderer uses <see cref="GlobalErrorCount"/> and
/// <see cref="GlobalWarningCount"/> to update its status badge on every
/// response without waiting for a separate <c>GetAllDiagnostics</c> request.
/// </summary>
/// <param name="AffectedEntries">Entry names whose diagnostics changed during this mutation.</param>
/// <param name="Diagnostics">Diagnostics for the affected entries (errors + warnings).</param>
/// <param name="GlobalErrorCount">Total project-wide error count after this mutation.</param>
/// <param name="GlobalWarningCount">Total project-wide warning count after this mutation.</param>
public sealed record MutationResponse(
    IReadOnlyList<string> AffectedEntries,
    IReadOnlyList<ProjectDiagnostic> Diagnostics,
    int GlobalErrorCount,
    int GlobalWarningCount);
