namespace Archetype.Tooling.Server.Handlers;

/// <summary>
/// Shared helpers for mutation handlers: re-validate and build the standard
/// <see cref="MutationResponse"/> (D28).
/// </summary>
internal static class MutationHelpers
{
    /// <summary>
    /// Rebuilds the reference graph, runs full validation, and returns a
    /// <see cref="MutationResponse"/> containing the diagnostics for the
    /// affected entries and the updated global counts.
    /// </summary>
    internal static MutationResponse RevalidateAndBuildResponse(
        ProjectState state,
        IReadOnlyList<string> affectedEntries)
    {
        // D28: initial implementation uses full re-validation.
        ReferenceGraph.Build(state);
        Validator.Validate(state);

        // Collect diagnostics for the affected entries.
        var scopedDiagnostics = state.Diagnostics
            .Where(d => affectedEntries.Contains(d.EntryName))
            .ToList();

        return new MutationResponse(
            AffectedEntries:    affectedEntries,
            Diagnostics:        scopedDiagnostics,
            GlobalErrorCount:   state.ErrorCount,
            GlobalWarningCount: state.WarningCount);
    }
}
