using System.Text.Json;
using Archetype.Tooling.Server.Export;

namespace Archetype.Tooling.Server.Handlers;

/// <summary>
/// Handles <c>ExportGodotClasses</c> — generates GDScript class files for
/// the Godot export package (D30).
/// Returns a <c>filename → content</c> map; the Electron main process writes
/// the files to disk.
/// </summary>
public sealed class ExportGodotClassesHandler(SidecarState sidecar)
{
    /// <summary>Returns <c>{ files: { filename: content, ... } }</c>.</summary>
    public object Handle(JsonElement? _)
    {
        var state = sidecar.State;

        if (state.ErrorCount > 0)
            throw new InvalidOperationException(
                $"{state.ErrorCount} error(s) must be fixed before exporting Godot classes.");

        var files = GodotClassGenerator.Generate(state);
        return new { files };
    }
}
