namespace Archetype.Tooling.Server;

// ---------------------------------------------------------------------------
//  LocalizationState — multi-locale string table (D27, D31)
// ---------------------------------------------------------------------------

/// <summary>
/// Holds all localisation data for the project.
/// <para>
/// <c>Strings</c> is a two-level map: <c>locale → (key → translated string)</c>.
/// The source language is stored as <c>SourceLanguage</c> and its entries
/// are the authoritative keys.  Each additional locale may be incomplete —
/// missing entries generate warnings (D31) at export time.
/// </para>
/// </summary>
public sealed class LocalizationState
{
    /// <summary>
    /// BCP-47 language tag for the source language (e.g. <c>"en"</c>).
    /// Defaults to <c>"en"</c>.
    /// </summary>
    public string SourceLanguage { get; set; } = "en";

    /// <summary>
    /// Translation tables keyed by locale then by translation key.
    /// <c>Strings["en"]["take-damage"] == "Deal {amount} damage to {target}"</c>.
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> Strings { get; set; } = [];

    /// <summary>
    /// Returns all keys that exist in the source language but are absent in
    /// the given <paramref name="locale"/>.
    /// </summary>
    public IReadOnlyList<string> MissingKeysForLocale(string locale)
    {
        if (!Strings.TryGetValue(SourceLanguage, out var sourceStrings))
            return [];
        if (!Strings.TryGetValue(locale, out var targetStrings))
            return [.. sourceStrings.Keys];

        return sourceStrings.Keys
            .Where(k => !targetStrings.ContainsKey(k))
            .ToList();
    }
}
