namespace Archetype.Core;

[Flags]
public enum CardType
{
    Spell,
    Unit,
    Structure,
    Node,
    Enemy
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
public enum KeywordOperandType
{
    Target,
    Integer,
    String
}

[Flags]
public enum CostType
{
    Resource, // From hand, with resource value
    Sacrifice, // From the board
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

