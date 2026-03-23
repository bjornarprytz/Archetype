namespace Archetype.Core;

// ---------------------------------------------------------------------------
//  StateFieldDecl — per-atom state map declaration (D38)
// ---------------------------------------------------------------------------

/// <summary>
/// The two types a declared state field can have.
/// <list type="bullet">
///   <item><see cref="Number"/> — an accumulator or modifier-adjusted property (numeric).</item>
///   <item><see cref="Bool"/> — a condition/tag (boolean).</item>
/// </list>
/// </summary>
public enum StateFieldType
{
    /// <summary>Numeric accumulator or modifier-adjusted property.</summary>
    Number,
    /// <summary>Boolean condition or tag.</summary>
    Bool,
}

/// <summary>
/// Declares a single named state field on an atom type definition (§2.6).
/// <para>
/// <b>Name:</b> unique within the containing <c>StateMapDeclarations</c> list.
/// A <see cref="StateFieldType.Number"/>-typed field that shares its name with
/// a static property denotes the modifier-adjusted computed value of that
/// property (§2.6 invariant).
/// </para>
/// <para>
/// <b>TextTemplate:</b> optional localizable natural-language description of
/// what this state field represents (e.g. <c>"current health"</c>,
/// <c>"is shielded"</c>).  Follows the same convention as
/// <see cref="KeywordDefinition.TextTemplate"/> — <c>null</c> means no rules
/// text is shown for this field.
/// </para>
/// </summary>
/// <param name="Name">The field name used at runtime with <c>modify-accumulator</c>,
/// <c>apply-condition</c>, etc.</param>
/// <param name="FieldType">Whether this field is a numeric accumulator or a boolean condition.</param>
/// <param name="TextTemplate">Optional localizable display text for this field.</param>
public sealed record StateFieldDecl(
    string Name,
    StateFieldType FieldType,
    string? TextTemplate = null);
