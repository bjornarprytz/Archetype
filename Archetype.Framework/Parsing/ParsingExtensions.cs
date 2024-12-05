using Archetype.Framework.Core;
using Archetype.Framework.State;

namespace Archetype.Framework.Parsing;

public static class ParsingExtensions
{
    /// <summary>
    /// Splits a path part into the part identifier and its arguments, separated by a colon (:).
    ///
    /// 
    /// </summary>
    /// <example>
    /// "part:arg1,arg2" -> ("part", ["arg1", "arg2"])
    /// </example>
    /// <param name="part"></param>
    /// <returns>A tuple where the first item is the part identifier and the second item is an array of the arguments.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static (string, string[]) SplitPart(this string part)
    {
        var parts = part.Split(':');
        
        if (parts.Length > 2)
            throw new InvalidOperationException($"Invalid part: {part}");
        
        return parts.Length == 1 ? (parts[0], Array.Empty<string>()) : (parts[0], parts[1].Split(','));
    }
    
    /// <summary>
    /// Tries to parse a word from a string. If the string is enclosed in single quotes, the word is the string without the quotes.
    /// </summary>
    /// <example>
    /// "'word'" -> "word"
    /// </example>
    /// <param name="text"></param>
    /// <param name="word"></param>
    /// <returns>True if text is a word</returns>
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
    
    
    public static ComparisonOperator ParseComparisonOperator(this string text)
    {
        return text switch
        {
            ">" => ComparisonOperator.GreaterThan,
            ">=" => ComparisonOperator.GreaterThanOrEqual,
            "<" => ComparisonOperator.LessThan,
            "<=" => ComparisonOperator.LessThanOrEqual,
            "=" => ComparisonOperator.Equal,
            "!=" => ComparisonOperator.NotEqual,
            "has" => ComparisonOperator.Contains,
            "!has" => ComparisonOperator.NotContains,
            _ => throw new InvalidOperationException($"Unknown comparison operator: {text}")
        };
    }
    
    public static void ValidateOrThrow<T>(this ComparisonOperator comparisonOperator)
    {
        if (typeof(T) == typeof(string) && comparisonOperator is not (ComparisonOperator.Equal or ComparisonOperator.NotEqual))
            throw new InvalidOperationException($"Invalid comparison operator for string: {comparisonOperator}");

        if (typeof(T) == typeof(int) &&
            comparisonOperator is ComparisonOperator.Contains or ComparisonOperator.NotContains)
        {
            throw new InvalidOperationException($"Invalid comparison operator for int: {comparisonOperator}");
        }
        
        if (typeof(T) == typeof(IEnumerable<IAtom>) &&
            comparisonOperator is not (ComparisonOperator.Contains or ComparisonOperator.NotContains))
        {
            throw new InvalidOperationException($"Invalid comparison operator for IEnumerable<IAtom>: {comparisonOperator}");
        }
    }
}