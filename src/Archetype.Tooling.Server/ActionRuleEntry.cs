using Archetype.Core;

namespace Archetype.Tooling.Server;

/// <summary>Action rule entry (before/after hooks for an action type).</summary>
public sealed class ActionRuleEntry
{
    /// <summary>DSL source for the before-action effect block (optional).</summary>
    public string? BeforeDsl { get; set; }

    /// <summary>Parsed before block; null when not set or on error.</summary>
    public EffectBlockDef? BeforeNode { get; set; }

    /// <summary>DSL source for the after-action effect block (optional).</summary>
    public string? AfterDsl { get; set; }

    /// <summary>Parsed after block; null when not set or on error.</summary>
    public EffectBlockDef? AfterNode { get; set; }

    /// <summary>Per-entry diagnostics.</summary>
    public List<ProjectDiagnostic> Diagnostics { get; set; } = [];
}
