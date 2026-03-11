using System.Text.Json;

namespace Archetype.Tooling.Server.Handlers;

/// <summary>
/// Handles <c>GetAllDiagnostics</c> — returns the full diagnostic list sorted
/// by severity then entry name (D28).
/// </summary>
public sealed class GetAllDiagnosticsHandler(SidecarState sidecar)
{
    /// <summary>Returns all diagnostics sorted by severity (errors first) then entry name.</summary>
    public object Handle(JsonElement? _)
    {
        var sorted = sidecar.State.Diagnostics
            .OrderBy(d => d.Severity == "error" ? 0 : 1)
            .ThenBy(d => d.EntryName)
            .ToList();

        return new { diagnostics = sorted };
    }
}
