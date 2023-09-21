namespace Archetype.Framework.Definitions;

[Flags]
public enum KeywordOperandType
{
    Integer,
    String
}

[Flags]
public enum KeywordType
{
    Effect,
    Feature,
    Aura,
    Reaction,
    DelayedEffect,
    Ability,
    Condition,
    Cost,
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

[Flags]
public enum Scope
{
    Self,
    Node,
    Global
}

