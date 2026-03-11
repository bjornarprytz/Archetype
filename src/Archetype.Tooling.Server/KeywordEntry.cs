using Archetype.Core;

namespace Archetype.Tooling.Server;

// ---------------------------------------------------------------------------
//  KeywordEntry — mutable in-memory authoring state for one keyword (D27)
// ---------------------------------------------------------------------------

/// <summary>
/// Mutable in-memory representation of a game-creator-defined keyword.
/// Holds both the DSL source string (canonical) and the parsed node tree
/// (derived; null when the DSL did not parse).
/// </summary>
public sealed class KeywordEntry
{
    /// <summary>Keyword name (e.g. <c>"take-damage"</c>).</summary>
    public string Name { get; set; } = "";

    /// <summary>Declared parameters in declaration order.</summary>
    public List<ParameterDecl> Parameters { get; set; } = [];

    /// <summary>Raw DSL source text; canonical form stored in the project file.</summary>
    public string BodyDsl { get; set; } = "";

    /// <summary>
    /// Parsed expression tree derived from <see cref="BodyDsl"/>.
    /// Null when <see cref="BodyDsl"/> contains a syntax error.
    /// </summary>
    public KeywordNode? BodyNode { get; set; }

    /// <summary>
    /// Optional text template for card text rendering (D11).
    /// E.g. <c>"Deal {amount} damage to {target}"</c>.
    /// </summary>
    public string? TextTemplate { get; set; }

    /// <summary>
    /// Signal generation behaviour for Godot export (D30).
    /// Tooling-only — not written to the exported GameDefinition.
    /// </summary>
    public SignalBehaviour SignalBehaviour { get; set; } = SignalBehaviour.Default;

    /// <summary>
    /// Per-entry diagnostics (parse errors, type errors).
    /// Reset on each validation pass.
    /// </summary>
    public List<ProjectDiagnostic> Diagnostics { get; set; } = [];
}

/// <summary>
/// Controls whether this keyword generates a GDScript signal in the Godot export (D30).
/// </summary>
public enum SignalBehaviour
{
    /// <summary>
    /// Apply the default derivation rules: suppress built-in primitives;
    /// include game-creator keywords referenced in card effect blocks.
    /// </summary>
    Default,

    /// <summary>
    /// Suppress signal generation for this keyword regardless of inclusion rules
    /// (<c>[NoSignal]</c> annotation).
    /// </summary>
    Suppress,

    /// <summary>
    /// Force signal generation for this keyword even if it is a built-in primitive
    /// (<c>[Signal]</c> annotation).
    /// </summary>
    ForceInclude,
}
