using System.Text.Json;
using Archetype.Core;

namespace Archetype.Tooling.Server.Handlers;

/// <summary>
/// Handles <c>GetCompletions</c> — returns context-sensitive completion items
/// for a DSL field at a cursor offset (D28, must respond within 100ms).
/// </summary>
public sealed class GetCompletionsHandler(SidecarState sidecar)
{
    /// <summary>
    /// Params: <c>entryKind</c>, <c>entryName</c>, <c>dsl</c>, <c>cursorOffset</c>.
    /// </summary>
    public object Handle(JsonElement? p)
    {
        var entryKind    = p?.GetProperty("entryKind").GetString() ?? "";
        var entryName    = p?.GetProperty("entryName").GetString() ?? "";
        var dsl          = p?.GetProperty("dsl").GetString() ?? "";
        var cursorOffset = p?.GetProperty("cursorOffset").GetInt32() ?? dsl.Length;

        var state = sidecar.State;

        // Determine which parameters are in scope for this entry.
        var inScopeParams = GetInScopeParams(state, entryKind, entryName);

        // Determine what's been typed so far (prefix before cursor).
        string prefix = ExtractPrefix(dsl, cursorOffset);

        var items = new List<object>();

        // Parameter completions
        foreach (var param in inScopeParams)
        {
            if (param.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                items.Add(new
                {
                    label      = param.Name,
                    kind       = "parameter",
                    detail     = param.Type.ToString(),
                    insertText = param.Name,
                });
        }

        // Keyword completions (game-creator + built-ins)
        foreach (var kwName in state.AllKeywordNames())
        {
            if (!kwName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;

            // Resolve the definition for detail text.
            string detail = "";
            string insertText = kwName;

            if (state.Keywords.TryGetValue(kwName, out var kw))
            {
                detail = BuildSignature(kwName, kw.Parameters);
                insertText = BuildInsertText(kwName, kw.Parameters);
            }
            else
            {
                var builtin = BuiltInKeywords.All.FirstOrDefault(k => k.Name == kwName);
                if (builtin is not null)
                {
                    detail = BuildSignature(kwName, builtin.Parameters.ToList());
                    insertText = BuildInsertText(kwName, builtin.Parameters.ToList());
                }
            }

            items.Add(new
            {
                label      = kwName,
                kind       = "keyword",
                detail,
                insertText,
            });
        }

        return new { items };
    }

    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------

    private static List<ParameterDecl> GetInScopeParams(
        ProjectState state, string entryKind, string entryName)
    {
        if (entryKind == "keyword" && state.Keywords.TryGetValue(entryName, out var kw))
            return kw.Parameters;
        return [];
    }

    private static string ExtractPrefix(string dsl, int offset)
    {
        if (offset > dsl.Length) offset = dsl.Length;
        int start = offset;
        while (start > 0 && IsIdentChar(dsl[start - 1])) start--;
        return dsl[start..offset];
    }

    private static bool IsIdentChar(char c) =>
        char.IsLetterOrDigit(c) || c == '_' || c == '-';

    private static string BuildSignature(
        string name, List<ParameterDecl> parameters)
    {
        var paramStr = string.Join(", ", parameters.Select(p => $"{p.Name}: {p.Type}"));
        return $"{name}({paramStr})";
    }

    private static string BuildInsertText(
        string name, List<ParameterDecl> parameters)
    {
        if (parameters.Count == 0) return $"{name}()";
        // Monaco snippet syntax: $1, $2, ...
        var slots = parameters.Select((p, i) => $"${{{i + 1}:{p.Name}}}");
        return $"{name}({string.Join(", ", slots)})";
    }
}
