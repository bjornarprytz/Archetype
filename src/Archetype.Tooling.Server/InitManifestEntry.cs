using Archetype.Core;

namespace Archetype.Tooling.Server;

/// <summary>InitManifest entry — authoring-time state of the default game setup.</summary>
public sealed class InitManifestEntry
{
    /// <summary>Zone specifications in provisioning order.</summary>
    public List<ZoneSpec> Zones { get; set; } = [];

    /// <summary>Card specifications in provisioning order.</summary>
    public List<CardSpec> Cards { get; set; } = [];

    /// <summary>Player initial state specifications.</summary>
    public List<PlayerStateSpec> Players { get; set; } = [];

    /// <summary>Per-entry diagnostics.</summary>
    public List<ProjectDiagnostic> Diagnostics { get; set; } = [];
}
