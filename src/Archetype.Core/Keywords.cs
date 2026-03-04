namespace Archetype.Core;

// ---------------------------------------------------------------------------
//  KeywordNode — immutable expression tree (D2)
// ---------------------------------------------------------------------------

/// <summary>
/// Base type for all nodes in a keyword expression tree.  The same tree
/// drives both execution (via the interpreter in <c>Archetype.Engine</c>) and
/// text rendering (via <c>TextRenderer</c> in <c>Archetype.Text</c>), satisfying
/// the dual-use invariant in §1.1.
/// </summary>
public abstract record KeywordNode;

/// <summary>
/// Refers to a declared parameter by name.  Resolves to its bound value in
/// the current <c>Bindings</c> during execution, or renders as the parameter
/// label during definition-time text rendering.
/// </summary>
/// <param name="Name">The declared parameter name to look up.</param>
public sealed record ParameterRef(string Name) : KeywordNode;

/// <summary>
/// A constant value hardcoded in the keyword definition.  Can hold a
/// <see cref="double"/>, <see cref="bool"/>, <see cref="string"/>, or an
/// <see cref="AtomId"/> referencing a known atom.
/// </summary>
/// <param name="Value">The literal value.</param>
public sealed record Literal(object Value) : KeywordNode;

/// <summary>
/// Calls another keyword (built-in or game-creator-defined) with the given
/// argument nodes.  The keyword name is looked up in
/// <c>GameDefinition.Keywords</c> at execution time.
/// </summary>
/// <param name="KeywordName">Name of the keyword to invoke.</param>
/// <param name="Args">Argument expression nodes.</param>
public sealed record Invocation(string KeywordName, params KeywordNode[] Args) : KeywordNode;

// ---------------------------------------------------------------------------
//  ParameterDecl — typed parameter declaration (D2 / A14)
// ---------------------------------------------------------------------------

/// <summary>
/// The engine's type vocabulary for keyword parameters.  <see cref="Atom"/>
/// is the generic atom type; <see cref="Card"/>, <see cref="Zone"/>,
/// <see cref="Player"/>, and <see cref="Session"/> are atom subtypes that
/// trigger stricter authoring-time checks.
/// </summary>
public enum TypeName
{
    /// <summary>Generic atom reference (any <see cref="AtomKind"/>).</summary>
    Atom,
    /// <summary>Atom restricted to <see cref="AtomKind.Card"/>.</summary>
    Card,
    /// <summary>Atom restricted to <see cref="AtomKind.Zone"/>.</summary>
    Zone,
    /// <summary>Atom restricted to <see cref="AtomKind.Player"/>.</summary>
    Player,
    /// <summary>Atom restricted to <see cref="AtomKind.Session"/>.</summary>
    Session,
    /// <summary>Numeric value (double).</summary>
    Number,
    /// <summary>Boolean value.</summary>
    Boolean,
    /// <summary>Name of a named condition.</summary>
    ConditionName,
    /// <summary>Name of a property/accumulator.</summary>
    PropertyName,
    /// <summary>An opaque contribution ID returned by mutation primitives.</summary>
    ContributionId,
    /// <summary>A <see cref="LifetimeSpec"/> data value.</summary>
    Lifetime,
    /// <summary>An <see cref="EffectBlockDef"/> passed as a parameter.</summary>
    EffectBlock,
    /// <summary>
    /// A named card definition — validated at load time against
    /// <c>GameDefinition.CardDefinitions</c>.
    /// </summary>
    CardDefinitionName,
    /// <summary>
    /// A named zone definition — validated at load time against
    /// <c>GameDefinition.ZoneDefinitions</c>.
    /// </summary>
    ZoneDefinitionName,
    /// <summary>
    /// A reference to a <see cref="GameEvent"/> node in the event log,
    /// accessible through <c>event-arg</c>.
    /// </summary>
    EventRef,
    /// <summary>An ordered collection of atoms (returned by <c>shuffle</c>, etc.).</summary>
    Collection,
}

/// <summary>
/// Declares a single parameter on a <see cref="KeywordDefinition"/>.
/// <para>
/// Built-in keywords may additionally specify <see cref="AtomKindRestriction"/>
/// to constrain a generic <c>Atom</c>-typed parameter to a subset of atom
/// kinds.  Game-creator-defined keywords may not set this field.
/// </para>
/// </summary>
/// <param name="Name">The parameter name used in <c>ParameterRef</c> nodes.</param>
/// <param name="Type">The declared type of this parameter.</param>
/// <param name="AtomKindRestriction">
/// Optional restriction on atom kind.  When non-null, the type-checker at
/// authoring time requires the argument to resolve statically to one of the
/// listed atom kinds.
/// </param>
public sealed record ParameterDecl(
    string Name,
    TypeName Type,
    AtomKind[]? AtomKindRestriction = null);

// ---------------------------------------------------------------------------
//  KeywordDefinition — a complete keyword (D2)
// ---------------------------------------------------------------------------

/// <summary>
/// A complete keyword definition.  Used for both primitive (built-in) and
/// composite (game-creator-defined) keywords.
/// <para>
/// Primitive keywords carry a <c>null</c> <see cref="Body"/> with a sentinel
/// <see cref="PrimitiveSentinel"/> marking the engine handler name.  Composite
/// keywords carry a real <see cref="Body"/> expression tree and
/// <c>null</c> sentinel.
/// </para>
/// </summary>
public sealed record KeywordDefinition(
    string Name,
    ParameterDecl[] Parameters,
    TypeName ReturnType,
    string Description,
    KeywordNode? Body = null,
    string? PrimitiveSentinel = null,
    string? TextTemplate = null)
{
    /// <summary>
    /// Returns <c>true</c> when this definition represents an engine-level
    /// mutation primitive rather than a game-creator-defined composite.
    /// </summary>
    public bool IsPrimitive => PrimitiveSentinel is not null;
}

// ---------------------------------------------------------------------------
//  EffectBlockDef and EffectBlockStep (D3 addendum)
// ---------------------------------------------------------------------------

/// <summary>
/// An ordered sequence of keyword invocations that executes atomically.
/// No triggers or state-based effects resolve mid-block.
/// </summary>
/// <param name="Steps">The steps to execute in order.</param>
public sealed record EffectBlockDef(IReadOnlyList<EffectBlockStep> Steps)
{
    /// <summary>An empty effect block (no-op).</summary>
    public static readonly EffectBlockDef Empty =
        new(Array.Empty<EffectBlockStep>());
}

/// <summary>
/// A single step within an <see cref="EffectBlockDef"/>.  Specifies which
/// keyword to invoke, what argument expressions to pass, and an optional
/// name to which the return value is bound for later steps.
/// </summary>
/// <param name="KeywordName">Name of the keyword to invoke.</param>
/// <param name="ArgNodes">Argument expression nodes.</param>
/// <param name="BindTo">
/// Optional: if non-null, the return value of this step is stored in
/// <c>ExecutionContext.Bindings</c> under this name and is accessible to
/// subsequent steps in the same block via <see cref="ParameterRef"/>.
/// </param>
public sealed record EffectBlockStep(
    string KeywordName,
    KeywordNode[] ArgNodes,
    string? BindTo = null);
