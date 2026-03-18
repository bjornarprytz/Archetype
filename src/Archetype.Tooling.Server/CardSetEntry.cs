namespace Archetype.Tooling.Server;

/// <summary>Card set entry (a named grouping of cards).</summary>
public sealed class CardSetEntry
{
    /// <summary>Card set name.</summary>
    public string Name { get; set; } = "";

    /// <summary>Card definition names belonging to this set.</summary>
    public List<string> Cards { get; set; } = [];

    /// <summary>Per-entry diagnostics.</summary>
    public List<ProjectDiagnostic> Diagnostics { get; set; } = [];
}
