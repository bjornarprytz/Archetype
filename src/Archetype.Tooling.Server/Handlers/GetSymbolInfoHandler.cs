using System.Text.Json;

namespace Archetype.Tooling.Server.Handlers;

/// <summary>
/// Handles <c>GetSymbolInfo</c> — identifies the keyword at a cursor offset
/// and returns its definition location and reverse references (D28).
/// </summary>
public sealed class GetSymbolInfoHandler(SidecarState sidecar)
{
    /// <summary>
    /// Params: <c>entryKind</c>, <c>entryName</c>, <c>cursorOffset</c>.
    /// </summary>
    public object Handle(JsonElement? p)
    {
        var entryKind    = p?.GetProperty("entryKind").GetString()
            ?? throw new ArgumentException("GetSymbolInfo: 'entryKind' required.");
        var entryName    = p?.GetProperty("entryName").GetString()
            ?? throw new ArgumentException("GetSymbolInfo: 'entryName' required.");
        var cursorOffset = p?.GetProperty("cursorOffset").GetInt32() ?? 0;

        var state = sidecar.State;

        // Retrieve the DSL for the entry.
        string? dsl = entryKind switch
        {
            "keyword" => state.Keywords.GetValueOrDefault(entryName)?.BodyDsl,
            "card"    => state.Cards.GetValueOrDefault(entryName)?.PrimaryEffectDsl,
            _         => null,
        };

        if (dsl is null)
            return new { symbol = (string?)null };

        // Find which token is at the cursor.
        var symbol = FindSymbolAtOffset(dsl, cursorOffset);
        if (symbol is null)
            return new { symbol = (string?)null };

        // Find the definition entry.
        object? definition = null;
        if (state.Keywords.ContainsKey(symbol))
            definition = new { entryKind = "keyword", entryName = symbol };
        else if (state.Cards.ContainsKey(symbol))
            definition = new { entryKind = "card", entryName = symbol };

        // Find callers from the reference graph.
        state.UsedBy.TryGetValue(symbol, out var callers);
        var referencedBy = (callers ?? [])
            .Select(c => new { entryName = c })
            .ToList();

        return new { symbol, kind = "keyword", definition, referencedBy };
    }

    /// <summary>
    /// Finds the identifier token in <paramref name="dsl"/> that covers
    /// <paramref name="offset"/>.
    /// </summary>
    private static string? FindSymbolAtOffset(string dsl, int offset)
    {
        if (offset > dsl.Length) return null;

        // Walk backwards to find the start of the identifier.
        int start = offset;
        while (start > 0 && IsIdentChar(dsl[start - 1])) start--;

        // Walk forwards to find the end.
        int end = offset;
        while (end < dsl.Length && IsIdentChar(dsl[end])) end++;

        if (start == end) return null;
        return dsl[start..end];
    }

    private static bool IsIdentChar(char c) =>
        char.IsLetterOrDigit(c) || c == '_' || c == '-';
}
