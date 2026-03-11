using System.Text.Json;

namespace Archetype.Tooling.Server.Handlers;

/// <summary>
/// Handles <c>UpdateKeywordBody</c> — parses new DSL for a keyword body,
/// updates <see cref="ProjectState"/>, and returns scoped diagnostics.
/// </summary>
public sealed class UpdateKeywordBodyHandler(SidecarState sidecar)
{
    /// <summary>Applies the DSL update and returns a <see cref="MutationResponse"/>.</summary>
    public object Handle(JsonElement? p)
    {
        var name = p?.GetProperty("keywordName").GetString()
            ?? throw new ArgumentException("UpdateKeywordBody: 'keywordName' required.");
        var dsl = p?.GetProperty("dsl").GetString()
            ?? throw new ArgumentException("UpdateKeywordBody: 'dsl' required.");

        var state = sidecar.State;

        // Create entry if it doesn't exist yet.
        if (!state.Keywords.TryGetValue(name, out var entry))
        {
            entry = new KeywordEntry { Name = name };
            state.Keywords[name] = entry;
        }

        entry.BodyDsl       = dsl;
        entry.Diagnostics   = [];

        var result = DslParser.Parse(dsl);
        if (result.IsSuccess)
        {
            entry.BodyNode = result.Node;
        }
        else
        {
            entry.BodyNode = null;
            entry.Diagnostics.Add(new ProjectDiagnostic(
                "keyword", name, "error",
                result.ErrorMessage ?? "Parse error.",
                result.ErrorOffset.HasValue
                    ? new DslRange(result.ErrorOffset.Value, dsl.Length)
                    : null));
        }

        // Affected: this keyword + everything that calls it.
        var affected = GetAffectedSet(state, name);
        return MutationHelpers.RevalidateAndBuildResponse(state, affected);
    }

    private static List<string> GetAffectedSet(ProjectState state, string name)
    {
        var set = new HashSet<string>(StringComparer.Ordinal) { name };
        if (state.UsedBy.TryGetValue(name, out var callers))
            foreach (var c in callers) set.Add(c);
        return [.. set];
    }
}
