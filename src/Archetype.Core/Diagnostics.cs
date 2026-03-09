namespace Archetype.Core;

// ---------------------------------------------------------------------------
//  Assert control types (D20) — inline-literal-only; not ParameterType values
// ---------------------------------------------------------------------------

/// <summary>
/// Controls what happens when an <c>assert</c> built-in fails.
/// <para>
/// <b>Inline-literal-only:</b> game creators may not declare keyword parameters
/// of this type.  It appears only as a literal argument to <c>assert</c>.
/// </para>
/// </summary>
public enum OnFail
{
    /// <summary>Log a diagnostic (if <see cref="NotifyFlag.On"/>), then continue execution.</summary>
    Continue = 0,
    /// <summary>Log a diagnostic (if <see cref="NotifyFlag.On"/>), then halt the block gracefully (no exception).</summary>
    Stop = 1,
    /// <summary>Log a diagnostic (if <see cref="NotifyFlag.On"/>), then raise <see cref="EngineException"/>.</summary>
    Panic = 2,
}

/// <summary>
/// Controls whether <see cref="IEngineObserver.OnDiagnostic"/> is called when
/// an <c>assert</c> fails.
/// <para>
/// <b>Inline-literal-only:</b> game creators may not declare keyword parameters
/// of this type.  It appears only as a literal argument to <c>assert</c>.
/// </para>
/// <para>
/// Note: when executing inside a cost body (<c>IsCostBody = true</c>), this
/// flag is ignored — cost bodies always behave as <c>off</c> regardless of the
/// call-site argument.
/// </para>
/// </summary>
public enum NotifyFlag
{
    /// <summary>Call <see cref="IEngineObserver.OnDiagnostic"/> on assertion failure.</summary>
    On = 0,
    /// <summary>Do NOT call <see cref="IEngineObserver.OnDiagnostic"/> on assertion failure.</summary>
    Off = 1,
}

// ---------------------------------------------------------------------------
//  DiagnosticEvent (D25)
// ---------------------------------------------------------------------------

/// <summary>
/// The kind of diagnostic raised.  Int-backed so future kinds can be added
/// without a breaking change to the observer interface.
/// </summary>
public enum DiagnosticKind
{
    /// <summary>An <c>assert</c> built-in evaluated its condition as false.</summary>
    AssertionFailed = 0,
}

/// <summary>
/// Diagnostic information produced when an <c>assert</c> built-in fails.
/// Delivered to <see cref="IEngineObserver.OnDiagnostic"/> before any
/// exception is raised.
/// <para>
/// <c>assert</c> never appends to the event log.  Diagnostics are a separate,
/// observer-only channel (D25).
/// </para>
/// </summary>
/// <param name="Kind">The kind of diagnostic.  Currently always <see cref="DiagnosticKind.AssertionFailed"/>.</param>
/// <param name="Message">Human-readable description of the failure.</param>
/// <param name="ConditionNode">
/// The failing condition AST node, or <c>null</c> when the node is unavailable.
/// </param>
/// <param name="OnFail">The <see cref="OnFail"/> value in effect at assertion time.</param>
/// <param name="Location">
/// Human-readable location string, e.g. <c>"energy_cost @ PlayCard"</c>.
/// </param>
public sealed record DiagnosticEvent(
    DiagnosticKind Kind,
    string Message,
    KeywordNode? ConditionNode,
    OnFail OnFail,
    string Location);
