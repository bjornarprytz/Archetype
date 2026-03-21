using Archetype.Core;

namespace Archetype.Tooling.Server;

/// <summary>State-based rule entry (evaluated each action for a fixpoint).</summary>
public sealed class StateBasedRuleEntry
{
    /// <summary>Rule name.</summary>
    public string Name { get; set; } = "";

    /// <summary>DSL source for the condition expression.</summary>
    public string ConditionDsl { get; set; } = "";

    /// <summary>Parsed condition; null on error.</summary>
    public KeywordNode? ConditionNode { get; set; }

    /// <summary>DSL source for the body effect block.</summary>
    public string BodyDsl { get; set; } = "";

    /// <summary>Parsed body block; null on error.</summary>
    public EffectBlockDef? BodyNode { get; set; }

    /// <summary>Per-entry diagnostics.</summary>
    public List<ProjectDiagnostic> Diagnostics { get; set; } = [];
}
