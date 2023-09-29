namespace Archetype.Framework.Definitions;

[Flags]
public enum KeywordOperandType
{
    Filter,
    Number,
    String
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
}

