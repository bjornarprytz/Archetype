using Archetype.Core;

namespace Archetype.Tooling.Server;

// ---------------------------------------------------------------------------
//  CardEntry — mutable in-memory authoring state for one card definition (D27)
// ---------------------------------------------------------------------------

/// <summary>
/// Mutable in-memory representation of a card definition during authoring.
/// Mirrors <see cref="CardDefinition"/> but uses DSL source strings rather
/// than parsed trees for all expression fields.
/// </summary>
public sealed class CardEntry
{
    /// <summary>Card definition name (e.g. <c>"goblin"</c>).</summary>
    public string Name { get; set; } = "";

    /// <summary>Static properties (type-specific, schema-driven).</summary>
    public Dictionary<string, object> StaticProperties { get; set; } = [];

    /// <summary>DSL source for the primary effect block.</summary>
    public string PrimaryEffectDsl { get; set; } = "";

    /// <summary>Parsed primary effect block; null when DSL has errors.</summary>
    public EffectBlockDef? PrimaryEffectNode { get; set; }

    /// <summary>Additional named effect blocks (activated abilities).</summary>
    public List<NamedEffectEntry> AdditionalEffects { get; set; } = [];

    /// <summary>Static effects (standing conditions with optional triggers).</summary>
    public List<StaticEffectEntry> StaticEffects { get; set; } = [];

    /// <summary>DSL source for the activation condition guard.</summary>
    public string? ActivationConditionDsl { get; set; }

    /// <summary>Parsed activation condition; null when DSL has errors or not set.</summary>
    public KeywordNode? ActivationConditionNode { get; set; }

    /// <summary>Cost definitions for playing this card.</summary>
    public List<CostEntry> Costs { get; set; } = [];

    /// <summary>Flavour text (not rendered in rules text).</summary>
    public string? FlavourText { get; set; }

    /// <summary>Path to the art asset within the project directory.</summary>
    public string? ArtPath { get; set; }

    /// <summary>
    /// Crop region as [x, y, width, height] normalised 0..1.
    /// Null means full image.
    /// </summary>
    public float[]? ArtCropRegion { get; set; }

    /// <summary>Per-entry diagnostics. Reset on each validation pass.</summary>
    public List<ProjectDiagnostic> Diagnostics { get; set; } = [];
}

/// <summary>A named effect block entry within a card (activated ability).</summary>
public sealed class NamedEffectEntry
{
    /// <summary>Ability name.</summary>
    public string Name { get; set; } = "";

    /// <summary>DSL source for the activation condition guard.</summary>
    public string? ActivationConditionDsl { get; set; }

    /// <summary>Parsed activation condition.</summary>
    public KeywordNode? ActivationConditionNode { get; set; }

    /// <summary>Cost definitions for activating this ability.</summary>
    public List<CostEntry> Costs { get; set; } = [];

    /// <summary>DSL source for the effect body.</summary>
    public string BodyDsl { get; set; } = "";

    /// <summary>Parsed effect body; null on error.</summary>
    public EffectBlockDef? BodyNode { get; set; }
}

/// <summary>A cost definition entry within a card or ability.</summary>
public sealed class CostEntry
{
    /// <summary>DSL source for the cost body.</summary>
    public string BodyDsl { get; set; } = "";

    /// <summary>Parsed cost body; null on error.</summary>
    public EffectBlockDef? BodyNode { get; set; }

    /// <summary>Optional text template.</summary>
    public string? TextTemplate { get; set; }
}

/// <summary>A static effect entry within a card.</summary>
public sealed class StaticEffectEntry
{
    /// <summary>DSL source for the state-contribution block.</summary>
    public string ContributionDsl { get; set; } = "";

    /// <summary>Parsed contribution block; null on error.</summary>
    public EffectBlockDef? ContributionNode { get; set; }

    /// <summary>DSL source for the trigger condition (optional).</summary>
    public string? TriggerConditionDsl { get; set; }

    /// <summary>Parsed trigger condition; null when not set or on error.</summary>
    public KeywordNode? TriggerConditionNode { get; set; }

    /// <summary>
    /// The keyword name that this trigger subscribes to (e.g. <c>"take-damage"</c>).
    /// Required when a trigger is defined; null otherwise.
    /// </summary>
    public string? TriggerEventKeyword { get; set; }

    /// <summary>
    /// How far into the event log the trigger condition can look (default: <c>ThisAction</c>).
    /// </summary>
    public TriggerScope TriggerScope { get; set; } = TriggerScope.ThisAction;

    /// <summary>DSL source for the triggered block (optional).</summary>
    public string? TriggerBodyDsl { get; set; }

    /// <summary>Parsed triggered body; null when not set or on error.</summary>
    public EffectBlockDef? TriggerBodyNode { get; set; }

    /// <summary>DSL source for the lifetime spec (optional).</summary>
    public string? LifetimeDsl { get; set; }

    /// <summary>
    /// Parsed lifetime spec; null when <see cref="LifetimeDsl"/> is not set,
    /// is empty, or failed to parse.  Null means permanent (D27).
    /// </summary>
    public LifetimeSpec? LifetimeNode { get; set; }

    /// <summary>Per-entry diagnostics. Reset on each validation pass.</summary>
    public List<ProjectDiagnostic> Diagnostics { get; set; } = [];
}
