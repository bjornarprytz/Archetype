using System.Text.Json;
using Archetype.Tooling.Server.Export;

namespace Archetype.Tooling.Server.Handlers;

/// <summary>
/// Handles <c>ExportGameDefinition</c> — exports the current project as a
/// strict <c>GameDefinition</c> JSON string (D27, D31).
/// </summary>
public sealed class ExportGameDefinitionHandler(SidecarState sidecar)
{
    /// <summary>
    /// Params: optional <c>force</c> (bool, default false) — bypasses the
    /// missing-translation confirmation gate (D31).
    /// </summary>
    public object Handle(JsonElement? p)
    {
        bool force = p?.TryGetProperty("force", out var f) == true && f.GetBoolean();

        var result = GameDefinitionExporter.Export(sidecar.State, force);

        if (result.IsBlocked)
            throw new InvalidOperationException(result.ErrorMessage!);

        if (result.HasMissingTranslations)
        {
            // Return the missing-translation summary so the renderer can show
            // the D31 confirmation dialog.
            return new
            {
                missingTranslations = result.MissingTranslations
                    .Select(m => new { locale = m.Locale, missingCount = m.MissingCount })
                    .ToList(),
            };
        }

        return new { json = result.Json };
    }
}
