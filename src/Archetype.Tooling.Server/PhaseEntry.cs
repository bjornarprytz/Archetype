using Archetype.Core;

namespace Archetype.Tooling.Server;

/// <summary>Phase definition entry.</summary>
public sealed class PhaseEntry
{
    /// <summary>Phase name.</summary>
    public string Name { get; set; } = "";

    /// <summary>DSL source for the phase init block (optional).</summary>
    public string? InitDsl { get; set; }

    /// <summary>Parsed init block; null when not set or on error.</summary>
    public EffectBlockDef? InitNode { get; set; }

    /// <summary>DSL source for the phase cleanup block (optional).</summary>
    public string? CleanupDsl { get; set; }

    /// <summary>Parsed cleanup block; null when not set or on error.</summary>
    public EffectBlockDef? CleanupNode { get; set; }

    /// <summary>Per-entry diagnostics.</summary>
    public List<ProjectDiagnostic> Diagnostics { get; set; } = [];
}
