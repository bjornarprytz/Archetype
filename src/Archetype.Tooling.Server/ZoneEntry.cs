namespace Archetype.Tooling.Server;

/// <summary>Zone definition entry.</summary>
public sealed class ZoneEntry
{
    /// <summary>Zone definition name.</summary>
    public string Name { get; set; } = "";

    /// <summary>Static properties.</summary>
    public Dictionary<string, object> StaticProperties { get; set; } = [];

    /// <summary>Per-entry diagnostics.</summary>
    public List<ProjectDiagnostic> Diagnostics { get; set; } = [];
}
