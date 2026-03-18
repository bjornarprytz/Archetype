namespace Archetype.Tooling.Server;

/// <summary>Player definition entry.</summary>
public sealed class PlayerEntry
{
    /// <summary>Player name.</summary>
    public string Name { get; set; } = "";

    /// <summary>Static properties.</summary>
    public Dictionary<string, object> StaticProperties { get; set; } = [];

    /// <summary>Per-entry diagnostics.</summary>
    public List<ProjectDiagnostic> Diagnostics { get; set; } = [];
}
