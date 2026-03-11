using System.Text.Json;

namespace Archetype.Tooling.Server.Handlers;

/// <summary>
/// Handles <c>GetReferenceGraph</c> — returns nodes and edges for the keyword
/// composition graph (D28).
/// </summary>
public sealed class GetReferenceGraphHandler(SidecarState sidecar)
{
    /// <summary>Returns graph nodes (all keywords) and edges (calls).</summary>
    public object Handle(JsonElement? _)
    {
        var state = sidecar.State;

        // Nodes: all game-creator keywords + referenced built-ins.
        var nodeNames = new HashSet<string>(state.Keywords.Keys, StringComparer.Ordinal);
        foreach (var (callee, _) in state.UsedBy)
            nodeNames.Add(callee);

        var nodes = nodeNames.Select(n => new
        {
            id      = n,
            kind    = state.Keywords.ContainsKey(n) ? "gameCreator" : "builtin",
        }).ToList();

        // Edges: derived from UsedBy (reversed: usedBy means "callee → caller").
        var edges = new List<object>();
        foreach (var (callee, callers) in state.UsedBy)
        {
            foreach (var caller in callers)
                edges.Add(new { source = caller, target = callee });
        }

        return new { nodes, edges };
    }
}
