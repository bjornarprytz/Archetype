namespace Archetype.Core;

// ---------------------------------------------------------------------------
//  CostDef — cost definition type (D20)
// ---------------------------------------------------------------------------

/// <summary>
/// Represents a single cost requirement on a card or ability.
/// <para>
/// A <see cref="CostDef"/> has no separate affordability evaluation function.
/// Instead, affordability is signalled by <c>assert</c> calls inside
/// <see cref="Body"/>.  When validation runs (see <c>CostValidator</c>),
/// all cost bodies are combined into a single composite block and executed
/// against a lightweight clone of <see cref="GameState"/> with
/// <c>IsCostBody = true</c>.  Any <c>assert</c> failure raises
/// <see cref="EngineException"/>, which the validator catches and translates
/// to <c>IsValid = false</c>.
/// </para>
/// <para>
/// At real execution time, each cost body runs within the same action scope as
/// the primary effect — cost events appear in <c>events.this_action</c> (D23).
/// </para>
/// </summary>
/// <param name="Body">
/// The effect block that pays this cost.  May contain <c>assert</c> calls to
/// signal un-affordability and mutation primitives to consume resources.
/// </param>
/// <param name="Parameters">
/// Player-provided arguments for this cost (e.g. which card to discard).
/// Bound from <c>PlayerAction.CostChoices</c> by parameter name.
/// </param>
/// <param name="TextTemplate">
/// Optional localized description of this cost for player-facing display.
/// Supports <c>{paramName}</c> placeholders.  When <c>null</c>, the engine
/// falls back to structural rendering of <see cref="Body"/>.
/// </param>
public sealed record CostDef(
    EffectBlockDef Body,
    ParameterDecl[] Parameters,
    string? TextTemplate = null);
