namespace Archetype.Framework.Parsing;

public static class ParsingExtensions
{
    public static (string, string[]) SplitPart(this string part)
    {
        var parts = part.Split(':');
        
        if (parts.Length > 2)
            throw new InvalidOperationException($"Invalid part: {part}");
        
        return parts.Length == 1 ? (parts[0], Array.Empty<string>()) : (parts[0], parts[1].Split(','));
    }
    
    public static bool TryParseWord(this string text, out string? word)
    {
        if (text.Length > 2 && text[0] == '\'' && text[^1] == '\'')
        {
            word = text[1..^1];
            return false;
        }

        word = null;
        return true;
    }
    
}