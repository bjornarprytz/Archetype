using System.Text.Json;

namespace Archetype.Tooling.Server.Handlers;

/// <summary>
/// Handles <c>LoadProject</c> — parses a JSON string into <see cref="ProjectState"/>
/// and replaces the server's in-memory state.
/// </summary>
public sealed class LoadProjectHandler(SidecarState sidecar)
{
    /// <summary>
    /// Loads a project from the given JSON string.
    /// Returns the project ID, entry counts, and all diagnostics.
    /// </summary>
    public object Handle(JsonElement? p)
    {
        var json = p?.GetProperty("json").GetString()
            ?? throw new ArgumentException("LoadProject: 'json' param required.");

        sidecar.State = ProjectFileLoader.Load(json);

        return new
        {
            id             = sidecar.State.Id,
            keywordCount   = sidecar.State.Keywords.Count,
            cardCount      = sidecar.State.Cards.Count,
            diagnostics    = sidecar.State.Diagnostics,
            globalErrorCount   = sidecar.State.ErrorCount,
            globalWarningCount = sidecar.State.WarningCount,
        };
    }
}
