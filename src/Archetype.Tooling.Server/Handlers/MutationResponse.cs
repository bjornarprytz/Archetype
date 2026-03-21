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
public sealed record MutationResponse(
    /// <summary>Entry names whose diagnostics changed during this mutation.</summary>
    IReadOnlyList<string> AffectedEntries,
    /// <summary>Diagnostics for the affected entries (errors + warnings).</summary>
    IReadOnlyList<ProjectDiagnostic> Diagnostics,
    /// <summary>Total project-wide error count after this mutation.</summary>
    int GlobalErrorCount,
    /// <summary>Total project-wide warning count after this mutation.</summary>
    int GlobalWarningCount);
