using System.Text.Json;

namespace Archetype.Tooling.Server.Handlers;

/// <summary>
/// Handles <c>SaveProject</c> — serialises the in-memory state to a JSON string.
/// The Electron main process writes the string to disk (D26 file I/O split).
/// </summary>
public sealed class SaveProjectHandler(SidecarState sidecar)
{
    /// <summary>Serialises and returns the project as a JSON string.</summary>
    public object Handle(JsonElement? _) =>
        new { json = ProjectFileSerializer.Serialize(sidecar.State) };
}
