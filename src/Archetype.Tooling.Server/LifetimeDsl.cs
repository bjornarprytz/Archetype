using Archetype.Core;

namespace Archetype.Tooling.Server;

// ---------------------------------------------------------------------------
//  LifetimeDsl — parses a lifetime DSL string into a LifetimeSpec (D27)
// ---------------------------------------------------------------------------

/// <summary>
/// Parses a lifetime DSL string into a <see cref="LifetimeSpec"/>.
/// <para>
/// Supported forms (conditions are OR-separated by <c>" | "</c>):
/// <list type="bullet">
///   <item><c>"3 turns"</c> — <see cref="TurnTimer"/></item>
///   <item><c>"2 triggers"</c> — <see cref="TriggerCount"/></item>
///   <item><c>"while &lt;expr&gt;"</c> — <see cref="WhileCondition"/> where
///   <c>&lt;expr&gt;</c> is a keyword DSL expression</item>
/// </list>
/// Multiple conditions may be combined: <c>"3 turns | 2 triggers"</c>.
/// A null or empty string means permanent and <see cref="Parse"/> returns
/// <see cref="LifetimeSpec.Permanent"/>.
/// </para>
/// </summary>
public static class LifetimeDsl
{
    /// <summary>
    /// Parses <paramref name="dsl"/> into a <see cref="LifetimeSpec"/>.
    /// Returns <see cref="LifetimeSpec.Permanent"/> when <paramref name="dsl"/>
    /// is null or empty.  Returns <c>null</c> when the string is non-empty but
    /// cannot be parsed (caller should record a diagnostic).
    /// </summary>
    public static LifetimeSpec? Parse(string? dsl)
    {
        if (string.IsNullOrWhiteSpace(dsl))
            return LifetimeSpec.Permanent;

        // Split on " | " to allow multiple conditions.
        var segments = dsl.Split(new[] { " | " }, StringSplitOptions.RemoveEmptyEntries);

        var conditions = new List<LifetimeCondition>();
        foreach (var seg in segments)
        {
            var trimmed = seg.Trim();
            var cond = ParseSingleCondition(trimmed);
            if (cond is null) return null; // one bad segment → whole spec fails
            conditions.Add(cond);
        }

        return conditions.Count == 0 ? LifetimeSpec.Permanent : new LifetimeSpec(conditions);
    }

    // -----------------------------------------------------------------------
    //  Internal helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Parses a single lifetime condition segment.
    /// Formats:
    ///   <c>N turns</c>   → <see cref="TurnTimer"/>
    ///   <c>N triggers</c> → <see cref="TriggerCount"/>
    ///   <c>while EXPR</c> → <see cref="WhileCondition"/> (EXPR parsed via <see cref="DslParser"/>)
    /// </summary>
    private static LifetimeCondition? ParseSingleCondition(string segment)
    {
        // "N turns"
        if (segment.EndsWith(" turns", StringComparison.OrdinalIgnoreCase))
        {
            var numStr = segment[..^" turns".Length].Trim();
            if (int.TryParse(numStr, out var turns) && turns > 0)
                return new TurnTimer(turns);
            return null;
        }

        // "N triggers"
        if (segment.EndsWith(" triggers", StringComparison.OrdinalIgnoreCase))
        {
            var numStr = segment[..^" triggers".Length].Trim();
            if (int.TryParse(numStr, out var count) && count > 0)
                return new TriggerCount(count);
            return null;
        }

        // "while EXPR"
        if (segment.StartsWith("while ", StringComparison.OrdinalIgnoreCase))
        {
            var exprDsl = segment["while ".Length..].Trim();
            if (string.IsNullOrEmpty(exprDsl)) return null;

            var result = DslParser.Parse(exprDsl);
            if (!result.IsSuccess || result.Node is null) return null;
            return new WhileCondition(result.Node);
        }

        return null;
    }

    /// <summary>
    /// Converts a <see cref="LifetimeSpec"/> back to its DSL string representation.
    /// Used by <see cref="ProjectFileSerializer"/> when serialising static effects.
    /// <see cref="LifetimeSpec.Permanent"/> serialises to an empty string.
    /// </summary>
    public static string Serialize(LifetimeSpec spec)
    {
        if (spec.IsPermanent) return "";

        var parts = spec.Conditions.Select(SerializeSingleCondition);
        return string.Join(" | ", parts);
    }

    private static string SerializeSingleCondition(LifetimeCondition cond) =>
        cond switch
        {
            TurnTimer t    => $"{t.Turns} turns",
            TriggerCount c => $"{c.Count} triggers",
            WhileCondition w => $"while {SerializeNode(w.Expression)}",
            _ => throw new InvalidOperationException(
                $"Unknown LifetimeCondition type: {cond.GetType().Name}"),
        };

    private static string SerializeNode(KeywordNode node) =>
        node switch
        {
            Invocation inv when inv.Args.Length == 0 => $"{inv.KeywordName}()",
            Invocation inv => $"{inv.KeywordName}({string.Join(", ", inv.Args.Select(SerializeNode))})",
            ParameterRef pr => pr.Name,
            Literal lit => lit.Value switch
            {
                string s => $"\"{s}\"",
                bool b   => b ? "true" : "false",
                _        => lit.Value?.ToString() ?? "null",
            },
            _ => node.ToString() ?? "",
        };
}
