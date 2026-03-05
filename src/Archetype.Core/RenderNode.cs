namespace Archetype.Core;

// ---------------------------------------------------------------------------
//  RenderNode — structured card text output (D11, D18)
// ---------------------------------------------------------------------------

/// <summary>
/// Base type for all nodes in a rendered card-text tree.  Produced by
/// <c>TextRenderer</c> in <c>Archetype.Text</c>; consumed by the host layer
/// (Godot) for display.
/// <para>
/// The tree is pure data — no engine dependencies, no behaviour.  It may be
/// traversed, serialised, or cached freely.
/// </para>
/// <para>
/// Four variants exist (D11 + D18):
/// <list type="bullet">
///   <item><see cref="TextSpan"/> — a literal string fragment (leaf).</item>
///   <item><see cref="CompositeNode"/> — a keyword invocation with both a
///   human-readable summary and a full recursive body expansion.</item>
///   <item><see cref="SequenceNode"/> — an ordered list of nodes (block steps,
///   argument lists, static effect parts).</item>
///   <item><see cref="RulesRef"/> — an inline cross-reference to a keyword
///   definition (leaf); the host renders it as a link and calls
///   <c>TextRenderer.Resolve</c> to expand it on demand.</item>
/// </list>
/// </para>
/// </summary>
public abstract record RenderNode;

/// <summary>
/// A leaf node containing a literal string fragment.  Produced by parameter
/// label substitution, literal value rendering, structural fallback text, and
/// lifetime descriptions.
/// </summary>
/// <param name="Text">The string content of this leaf.</param>
public sealed record TextSpan(string Text) : RenderNode;

/// <summary>
/// Represents a keyword invocation as a two-level node (D11).
/// <para>
/// <see cref="Summary"/> is the human-readable rendering from the locale file,
/// the <c>TextTemplate</c> on the keyword definition, or a structural fallback
/// — whichever is resolved first.  Only <see cref="Summary"/> varies with
/// locale; <see cref="Body"/> is always the full recursive structural expansion
/// of the keyword's composition tree.
/// </para>
/// <para>
/// The host's minimal implementation: render <see cref="Summary"/> only.
/// A richer host shows <see cref="Body"/> as an expandable tooltip.
/// </para>
/// </summary>
/// <param name="Summary">Human-readable card text from the resolved template.</param>
/// <param name="Body">Full recursive expansion of the keyword's composition tree.</param>
public sealed record CompositeNode(RenderNode Summary, RenderNode Body) : RenderNode;

/// <summary>
/// An ordered list of <see cref="RenderNode"/>s.  Used for effect block steps,
/// argument lists, static effect parts (contribution + trigger + lifetime), and
/// any other ordered grouping.  Separator and list formatting are
/// host responsibilities.
/// </summary>
/// <param name="Items">The ordered child nodes.</param>
public sealed record SequenceNode(IReadOnlyList<RenderNode> Items) : RenderNode;

/// <summary>
/// An inline cross-reference to a keyword definition (D18).  A leaf node: it
/// carries only the keyword name and the display text shown to the player.
/// The host renders it as an interactive link; when the player activates it,
/// the host calls <c>TextRenderer.Resolve</c> to obtain the linked keyword's
/// rendered definition on demand (lazy expansion).
/// <para>
/// Produced from template tags of the form <c>[display](keyword-name)</c>
/// (long form) or <c>[keyword-name]</c> (short form, where display text equals
/// the keyword name).
/// </para>
/// </summary>
/// <param name="Key">The keyword name to resolve via <c>TextRenderer.Resolve</c>.</param>
/// <param name="DisplayText">The text shown to the player for this link.</param>
public sealed record RulesRef(string Key, string DisplayText) : RenderNode;
