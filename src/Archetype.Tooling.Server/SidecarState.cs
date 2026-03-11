namespace Archetype.Tooling.Server;

/// <summary>
/// Holds the single mutable <see cref="ProjectState"/> for the sidecar's lifetime.
/// All handlers share this object; only one request runs at a time (stdin is
/// read sequentially) so no synchronisation is required.
/// </summary>
public sealed class SidecarState
{
    /// <summary>The currently loaded project state.</summary>
    public ProjectState State { get; set; } = new();
}
