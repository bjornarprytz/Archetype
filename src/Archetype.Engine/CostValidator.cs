using Archetype.Core;

namespace Archetype.Engine;

/// <summary>
/// Validates whether the costs of a candidate <see cref="PlayerAction"/> are
/// currently affordable, without mutating the live <see cref="GameState"/> (D21).
/// <para>
/// All cost bodies are combined into a single composite <see cref="EffectBlockDef"/>
/// and executed against a lightweight clone of the current state with
/// <see cref="ExecutionContext.IsCostBody"/> = <c>true</c>.  Any
/// <see cref="EngineException"/> (from a failing <c>assert</c>) is caught and
/// translates to <see cref="ValidationResult.IsValid"/> = <c>false</c>.
/// </para>
/// <para>
/// Cost texts (<see cref="ValidationResult.CostTexts"/>) are always resolved
/// regardless of the validation outcome, for player-facing display.
/// </para>
/// </summary>
internal static class CostValidator
{
    /// <summary>
    /// Validates the given list of costs against a clone of <paramref name="state"/>.
    /// </summary>
    /// <param name="costs">The costs to validate.  May be empty or null.</param>
    /// <param name="action">The candidate action (provides <c>CostChoices</c>).</param>
    /// <param name="state">The current live game state; not mutated.</param>
    /// <param name="definition">The game definition for keyword resolution.</param>
    /// <param name="strategies">Player strategies (needed by ExecutionContext).</param>
    /// <param name="randomSource">Random source (needed by ExecutionContext).</param>
    /// <param name="activePlayerName">The active player name for prompt dispatch.</param>
    /// <returns>A <see cref="ValidationResult"/> with affordability and cost texts.</returns>
    public static ValidationResult Validate(
        IReadOnlyList<CostDef>? costs,
        PlayerAction action,
        GameState state,
        GameDefinition definition,
        IReadOnlyDictionary<string, IPlayerStrategy> strategies,
        IRandomSource randomSource,
        string activePlayerName)
    {
        // Always resolve cost texts regardless of affordability.
        var costTexts = ResolveCostTexts(costs, action);

        // Short-circuit: no costs — always valid.
        if (costs is null || costs.Count == 0)
            return ValidationResult.Empty;

        // Combine all cost bodies into a single composite EffectBlockDef.
        // Steps are concatenated in declaration order (D21).
        var combinedSteps = costs
            .SelectMany(c => c.Body.Steps)
            .ToList();
        var combinedBlock = new EffectBlockDef(combinedSteps);

        // Clone the game state — validation must not mutate live state (D21).
        var cloneState = state.CloneForValidation();

        // Build cost bindings: merge CostChoices from the action into the
        // initial bindings so each CostDef.Parameter is resolvable.
        var costChoices = ExtractCostChoices(action);
        var bindings    = new Dictionary<string, object>(costChoices, StringComparer.Ordinal);

        // Build a minimal EventLog for the clone execution.  Cost validation
        // has no observer and produces no events that are kept.
        var cloneLog = new EventLog();
        cloneLog.OpenAction();

        var ctx = new ExecutionContext(
            cloneState,
            cloneLog,
            bindings,
            strategies,
            randomSource,
            definition,
            activePlayerName,
            observer: null)   // no observer — cost validation is silent (D21)
        {
            // Set IsCostBody so assert always panics silently on failure (D20).
            IsCostBody = true,
        };

        try
        {
            // Run synchronously: cost bodies must not have prompt suspension (D22 note).
            // The combined block executes atomically against the clone.
            var executor = new BlockExecutor();
            executor.ExecuteBlock(combinedBlock, ctx).GetAwaiter().GetResult();
            return new ValidationResult(IsValid: true, CostTexts: costTexts);
        }
        catch (EngineException)
        {
            // assert or any other runtime failure — costs are not affordable.
            return new ValidationResult(IsValid: false, CostTexts: costTexts);
        }
        finally
        {
            // Close the clone's action scope (cleanup only — clone is discarded).
            try { cloneLog.CloseAction(); } catch { /* ignore — clone is discarded */ }
        }
    }

    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Resolves cost texts for each <see cref="CostDef"/> in declaration order.
    /// When <see cref="CostDef.TextTemplate"/> is non-null, substitutes
    /// <c>{paramName}</c> placeholders from <c>CostChoices</c>.
    /// Falls back to the structural description when no template is set.
    /// Always returns one string per cost regardless of validation outcome.
    /// </summary>
    private static IReadOnlyList<string> ResolveCostTexts(
        IReadOnlyList<CostDef>? costs,
        PlayerAction action)
    {
        if (costs is null || costs.Count == 0)
            return Array.Empty<string>();

        var choices = ExtractCostChoices(action);
        var texts   = new List<string>(costs.Count);

        foreach (var cost in costs)
        {
            string text;
            if (cost.TextTemplate is not null)
            {
                // Substitute {paramName} placeholders with CostChoices values.
                text = cost.TextTemplate;
                foreach (var (name, value) in choices)
                    text = text.Replace($"{{{name}}}", value?.ToString() ?? string.Empty,
                                        StringComparison.Ordinal);
            }
            else
            {
                // Structural fallback: describe the cost body steps.
                text = string.Join(", ", cost.Body.Steps.Select(s => s.KeywordName));
            }
            texts.Add(text);
        }

        return texts;
    }

    /// <summary>
    /// Extracts the <c>CostChoices</c> dictionary from the action, or returns
    /// an empty dictionary when the action type has no cost choices.
    /// </summary>
    private static IReadOnlyDictionary<string, object> ExtractCostChoices(PlayerAction action) =>
        action switch
        {
            PlayCard       pc => pc.CostChoices ?? EmptyChoices,
            ActivateAbility aa => aa.CostChoices ?? EmptyChoices,
            _ => EmptyChoices,
        };

    private static readonly IReadOnlyDictionary<string, object> EmptyChoices =
        new Dictionary<string, object>(StringComparer.Ordinal);
}
