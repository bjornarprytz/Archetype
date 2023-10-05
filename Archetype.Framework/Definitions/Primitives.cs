namespace Archetype.Framework.Definitions;

[Flags]
public enum KeywordOperandParsedType
{
    Filter,
    Number,
    String,
    Word
}

[Flags]
public enum CostType
{
    Resource, // From hand, with resource value
    Sacrifice, // From the board
    Work, // From the board (like tapping)
    Discard, // From hand
    Mill, // From draw pile
    Exhaust, // From discard pile
    Coins, // From the player's pocket
}


public static class Helpers
{
    public static KeywordOperandParsedType GetParsedType<T>() // TODO: Use this somewhere (for validation?), or remove it
    {
        return typeof(T) switch
        {
            { } t when t == typeof(Filter) => KeywordOperandParsedType.Filter,
            { } t when t == typeof(int) => KeywordOperandParsedType.Number,
            { } t when t == typeof(string) => KeywordOperandParsedType.String | KeywordOperandParsedType.Word,
            _ => throw new InvalidOperationException($"Unknown operand type {typeof(T)}")
        };
    }
} 
