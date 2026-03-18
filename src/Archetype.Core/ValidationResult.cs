namespace Archetype.Core;

// ---------------------------------------------------------------------------
//  ValidationResult (D21, D22)
// ---------------------------------------------------------------------------

/// <summary>
/// The result of validating a candidate <see cref="PlayerAction"/> via
/// <see cref="AvailableActions.ValidateActionArgs"/>.
/// <para>
/// Cost texts are always populated regardless of validation outcome, so the
/// player-facing display can show costs even when they are unaffordable.
/// </para>
/// </summary>
/// <param name="IsValid">
/// <c>true</c> if all cost bodies for this action completed without raising
/// <see cref="EngineException"/>; <c>false</c> otherwise.
/// </param>
/// <param name="CostTexts">
/// One resolved text string per <see cref="CostDef"/>, in declaration order.
/// Populated from <see cref="CostDef.TextTemplate"/> (with placeholder
/// substitution) or a structural fallback when the template is null.
/// Always populated regardless of <see cref="IsValid"/>.
/// </param>
public sealed record ValidationResult(bool IsValid, IReadOnlyList<string> CostTexts)
{
    /// <summary>
    /// A pre-built result for actions with no costs: always valid, no cost texts.
    /// </summary>
    public static readonly ValidationResult Empty =
        new(IsValid: true, CostTexts: Array.Empty<string>());

    /// <summary>
    /// Convenience factory: creates an invalid result with the supplied cost texts.
    /// </summary>
    public static ValidationResult Invalid(IReadOnlyList<string> costTexts) =>
        new(IsValid: false, CostTexts: costTexts);
}
